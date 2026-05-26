import {Navigate, Route, Routes, useLocation, useNavigate} from 'react-router-dom';
import {useEffect, useMemo, useRef, useState} from 'react';

type Theme = 'light' | 'dark';

type TelegramUserData = {
    telegramId: number;
    userName?: string | null;
    firstName: string;
    lastName?: string | null;
    languageCode: string;
};

type UserDto = {
    id: string;
    telegramId: number;
    userName?: string | null;
    firstName: string;
    lastName?: string | null;
    languageCode: string;
    balance: number;
    role: string;
    createdAt: string;
    lastActiveAt: string;
    panelClients: Array<{
        id: string;
        email: string;
        uuid: string;
        subId: string;
        expiryTime: number;
        totalGB: number;
        limitIp: number;
        flow: string;
        enable: boolean;
        reset: number;
    }>;
    subscriptions: Array<{
        id: string;
        email: string;
        name: string;
        startedAt: string;
        endedAt: string;
        price: number;
        status: string;
        plans: string;
    }>;
};

type Plan = {
    name: 'Monthly' | 'SixMonths' | 'Year';
    title: string;
    price: string;
    summary: string;
};

type SubscriptionPlanOption = {
    value: 'Trial' | 'Monthly' | 'Quarterly' | 'Yearly';
    label: string;
};

type BalanceOperationKind = 'deposit' | 'subscription' | 'promo' | 'withdraw' | 'system';
type BalanceOperationFilter = 'all' | BalanceOperationKind;

type BalanceOperationEntry = {
    id: string;
    kind: BalanceOperationKind;
    title: string;
    amount: string;
    note: string;
    time: string;
};

type BalanceOperationApiResponse = {
    id: string;
    kind: BalanceOperationKind;
    title: string;
    amount: number;
    note: string;
    createdAt: string;
};

type ToastKind = 'success' | 'error' | 'info';

type ToastState = {
    id: number;
    title: string;
    message?: string;
    kind: ToastKind;
    closing?: boolean;
};

type BalanceFilterOption = {
    value: BalanceOperationFilter;
    label: string;
    description: string;
};

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5118';
const enableTelegramMock = import.meta.env.DEV || import.meta.env.VITE_ENABLE_TELEGRAM_MOCK === 'true';
const adminTelegramId = '1552110836';

const navItems = [
    {path: '/', title: 'Главная'},
    {path: '/balance', title: 'Баланс'},
    {path: '/connection', title: 'Подключение'},
    {path: '/payment', title: 'Оплата'},
    {path: '/admin', title: 'Админка'},
] as const;

const plans: Plan[] = [
    {
        name: 'Monthly',
        title: 'На месяц',
        price: '179 ₽',
        summary: 'Сбалансированный вариант для повседневного использования без лишней переплаты.',
    },
    {
        name: 'SixMonths',
        title: 'На 6 месяцев',
        price: '999 ₽',
        summary: 'Комфортный срок для стабильного доступа и редких продлений.',
    },
    {
        name: 'Year',
        title: 'На год',
        price: '1999 ₽',
        summary: 'Максимально выгодный вариант по стоимости одного платежа.',
    },
];

const purchasePlanMap: Record<Plan['name'], 'Monthly' | 'Quarterly' | 'Yearly'> = {
    Monthly: 'Monthly',
    SixMonths: 'Quarterly',
    Year: 'Yearly',
};

const adminSubscriptionPlans: SubscriptionPlanOption[] = [
    {value: 'Trial', label: 'Trial | 0 ₽'},
    {value: 'Monthly', label: 'Monthly | 179 ₽'},
    {value: 'Quarterly', label: 'Quarterly | 999 ₽'},
    {value: 'Yearly', label: 'Yearly | 1999 ₽'},
];

const balanceFilterOptions: BalanceFilterOption[] = [
    {value: 'all', label: 'Все операции', description: 'Показывает всю историю.'},
    {value: 'deposit', label: 'Пополнения', description: 'Входящие платежи и зачисления.'},
    {value: 'subscription', label: 'Подписки', description: 'Покупка и продление тарифов.'},
    {value: 'promo', label: 'Промокоды', description: 'Начисления по промокодам.'},
    {value: 'withdraw', label: 'Выводы', description: 'Списание и вывод средств.'},
    {value: 'system', label: 'Система', description: 'Админские и технические действия.'},
];

function readTelegramUserData(telegram: { initDataUnsafe?: { user?: any } } | null): TelegramUserData | null {
    const user = telegram?.initDataUnsafe?.user;
    if (user?.id) {
        return {
            telegramId: user.id as number,
            userName: user.username as string | undefined,
            firstName: user.first_name as string,
            lastName: user.last_name as string | undefined,
            languageCode: (user.language_code as string | undefined) ?? 'ru',
        };
    }

    if (!enableTelegramMock) {
        return null;
    }

    const params = new URLSearchParams(window.location.search);
    const telegramId = Number(params.get('telegramId') ?? '');
    const firstName = params.get('firstName');

    if (Number.isFinite(telegramId) && telegramId > 0 && firstName) {
        return {
            telegramId,
            userName: params.get('userName') ?? undefined,
            firstName,
            lastName: params.get('lastName') ?? undefined,
            languageCode: params.get('languageCode') ?? 'ru',
        };
    }

    const stored = window.localStorage.getItem('ayanami.telegramMockUser');
    if (stored) {
        try {
            const parsed = JSON.parse(stored) as TelegramUserData;
            if (parsed.telegramId && parsed.firstName) {
                return {
                    telegramId: parsed.telegramId,
                    userName: parsed.userName ?? undefined,
                    firstName: parsed.firstName,
                    lastName: parsed.lastName ?? undefined,
                    languageCode: parsed.languageCode ?? 'ru',
                };
            }
        } catch {
            // ignore
        }
    }

    return null;
}

function App() {
    const location = useLocation();
    const navigate = useNavigate();
    const [theme, setTheme] = useState<Theme>('light');
    const [manualTelegramId, setManualTelegramId] = useState(() => '');
    const [telegramIdInput, setTelegramIdInput] = useState('');
    const [balanceAmount, setBalanceAmount] = useState('179');

    // РАЗДЕЛЕННЫЕ СОСТОЯНИЯ ПРОФИЛЕЙ
    const [myProfile, setMyProfile] = useState<UserDto | null>(null);
    const [selectedUser, setSelectedUser] = useState<UserDto | null>(null);

    const [users, setUsers] = useState<UserDto[]>([]);
    const [balanceOperations, setBalanceOperations] = useState<BalanceOperationEntry[]>([]);
    const [balanceFilter, setBalanceFilter] = useState<BalanceOperationFilter>('all');
    const [balanceFilterOpen, setBalanceFilterOpen] = useState(false);
    const [balanceFilterClosing, setBalanceFilterClosing] = useState(false);
    const [status, setStatus] = useState('Готово');

    const [loading, setLoading] = useState(false);
    const [bootstrapped, setBootstrapped] = useState(false);
    const [menuOpen, setMenuOpen] = useState(false);
    const [subscriptionFlipped, setSubscriptionFlipped] = useState(false);
    const [adminSearchValue, setAdminSearchValue] = useState('');
    const [promoMode, setPromoMode] = useState<'days' | 'balance'>('days');
    const [promoCodeValue, setPromoCodeValue] = useState('');
    const [promoGrantValue, setPromoGrantValue] = useState('7');
    const [adminSelectedPlan, setAdminSelectedPlan] = useState<'Trial' | 'Monthly' | 'Quarterly' | 'Yearly'>('Trial');
    const [expandedAdminUserId, setExpandedAdminUserId] = useState<string | null>(null);
    const [bootstrapStep, setBootstrapStep] = useState('idle');
    const [bootstrapError, setBootstrapError] = useState('');
    const [bootstrapTrace, setBootstrapTrace] = useState<string[]>([]);
    const [bootstrapResponseBody, setBootstrapResponseBody] = useState('');
    const [debugOpen, setDebugOpen] = useState(false);
    const [toast, setToast] = useState<ToastState | null>(null);
    const toastTimerRef = useRef<number | null>(null);
    const [homeRevealToken, setHomeRevealToken] = useState(0);

    const telegram = useMemo(() => {
        const webApp = (window as Window & { Telegram?: { WebApp?: any } }).Telegram?.WebApp;
        return webApp ?? null;
    }, []);

    const initTelegramData = useMemo(() => readTelegramUserData(telegram), [telegram]);
    const manualBootstrapData = useMemo<TelegramUserData | null>(() => {
        const telegramId = Number(manualTelegramId.trim());
        if (!manualTelegramId.trim() || !Number.isFinite(telegramId) || telegramId <= 0) {
            return null;
        }

        return {
            telegramId,
            userName: null,
            firstName: `User ${telegramId}`,
            lastName: null,
            languageCode: 'ru',
        };
    }, [manualTelegramId]);

    const bootstrapData = initTelegramData ?? manualBootstrapData;

    // ПРОВЕРКА АДМИНА ТЕПЕРЬ ПО СВОЕМУ ПРОФИЛЮ
    const isAdmin = myProfile?.telegramId?.toString() === adminTelegramId;

    const currentSubscription = useMemo(() => {
        const subscriptions = myProfile?.subscriptions ?? [];

        if (subscriptions.length === 0) {
            return null;
        }

        return [...subscriptions]
                .sort((left, right) => new Date(right.endedAt).getTime() - new Date(left.endedAt).getTime())
                .find(subscription => subscription.status === 'Active')
            ?? [...subscriptions].sort((left, right) => new Date(right.endedAt).getTime() - new Date(left.endedAt).getTime())[0];
    }, [myProfile]);

    const currentPanelClient = useMemo(() => {
        const panelClients = myProfile?.panelClients ?? [];

        if (panelClients.length === 0) {
            return null;
        }

        const normalizedSubscriptionId = currentSubscription?.id.replace(/-/g, '').toLowerCase();

        if (normalizedSubscriptionId) {
            const matchedClient = panelClients.find(client => client.subId.toLowerCase() === normalizedSubscriptionId);
            if (matchedClient) {
                return matchedClient;
            }
        }

        return panelClients[0] ?? null;
    }, [myProfile, currentSubscription]);

    const filteredBalanceOperations = useMemo(() => {
        if (balanceFilter === 'all') {
            return balanceOperations;
        }

        return balanceOperations.filter(operation => operation.kind === balanceFilter);
    }, [balanceFilter, balanceOperations]);

    const activeBalanceFilter = useMemo(
        () => balanceFilterOptions.find(option => option.value === balanceFilter) ?? balanceFilterOptions[0],
        [balanceFilter]
    );
    const connectionSubId = currentPanelClient?.subId ?? currentSubscription?.id.replace(/-/g, '') ?? '';
    const connectionLink = connectionSubId
        ? `https://subs.ayanami-connect.online/t/${connectionSubId}`
        : 'https://subs.ayanami-connect.online/t/your-subscription-link';

    const rootClassName = theme === 'light' ? 'app app--light' : 'app app--dark';
    const showBootstrapDebug = isAdmin && (import.meta.env.DEV || import.meta.env.VITE_ENABLE_BOOTSTRAP_DEBUG === 'true');

    useEffect(() => {
        if (showBootstrapDebug && import.meta.env.DEV) {
            setDebugOpen(true);
        } else {
            setDebugOpen(false);
        }
    }, [showBootstrapDebug]);

    function showToast(title: string, kind: ToastKind = 'info', message?: string) {
        if (toastTimerRef.current !== null) {
            window.clearTimeout(toastTimerRef.current);
        }

        setToast({
            id: Date.now(),
            title,
            message,
            kind,
            closing: false,
        });

        toastTimerRef.current = window.setTimeout(() => {
            toastTimerRef.current = null;
            dismissToast();
        }, 3000);
    }

    function dismissToast() {
        if (toastTimerRef.current !== null) {
            window.clearTimeout(toastTimerRef.current);
            toastTimerRef.current = null;
        }

        setToast(current => (current ? {...current, closing: true} : current));

        window.setTimeout(() => {
            setToast(current => (current?.closing ? null : current));
        }, 180);
    }

    function replayHomeReveal() {
        setHomeRevealToken(previous => previous + 1);
    }

    function closeBalanceFilterMenu() {
        if (!balanceFilterOpen || balanceFilterClosing) {
            return;
        }

        setBalanceFilterClosing(true);
        window.setTimeout(() => {
            setBalanceFilterOpen(false);
            setBalanceFilterClosing(false);
        }, 180);
    }

    async function loadBalanceOperations(telegramId: string) {
        const result = await apiFetch(`/api/users/${telegramId}/operations`);

        if (!Array.isArray(result)) {
            setBalanceOperations([]);
            return;
        }

        const mappedOperations = (result as BalanceOperationApiResponse[]).map(operation => ({
            id: operation.id,
            kind: operation.kind,
            title: operation.title,
            amount: formatOperationAmount(operation.amount),
            note: operation.note,
            time: formatOperationTime(operation.createdAt),
        }));

        setBalanceOperations(mappedOperations);
    }

    function pushBootstrapTrace(entry: string) {
        setBootstrapTrace(previous => [entry, ...previous].slice(0, 6));
        console.log('[AyanamiConnect bootstrap]', entry);
    }

    function buildBootstrapDebugText() {
        return [
            `Step: ${bootstrapStep}`,
            `Status: ${status}`,
            `Telegram ID: ${telegramIdInput.trim() || manualTelegramId.trim() || '—'}`,
            `Source: ${initTelegramData ? 'telegram' : 'manual'}`,
            `Error: ${bootstrapError || '—'}`,
            `Raw body: ${bootstrapResponseBody || '—'}`,
            '',
            'Trace:',
            ...bootstrapTrace,
        ].join('\n');
    }

    useEffect(() => {
        telegram?.ready?.();
        telegram?.expand?.();

        if (telegram?.colorScheme === 'dark') {
            setTheme('dark');
        }
    }, [telegram]);

    useEffect(() => {
        return () => {
            if (toastTimerRef.current !== null) {
                window.clearTimeout(toastTimerRef.current);
            }
        };
    }, []);

    useEffect(() => {
        setMenuOpen(false);
    }, [location.pathname]);

    useEffect(() => {
        if (location.pathname === '/admin') {
            void loadAllUsers();
        }
    }, [location.pathname]);

    useEffect(() => {
        if (location.pathname === '/balance' && myProfile) {
            void loadBalanceOperations(String(myProfile.telegramId));
        }
    }, [location.pathname, myProfile]);

    useEffect(() => {
        if (!initTelegramData || bootstrapped) {
            return;
        }

        let cancelled = false;
        void bootstrapFromData(initTelegramData, () => cancelled);

        return () => {
            cancelled = true;
        };
    }, [bootstrapped, initTelegramData]);

    useEffect(() => {
        if (manualTelegramId.trim()) {
            window.localStorage.setItem('ayanami.manualTelegramId', manualTelegramId.trim());
        }
    }, [manualTelegramId]);

    function submitTelegramId() {
        const parsed = Number(manualTelegramId.trim());
        if (!Number.isFinite(parsed) || parsed <= 0) {
            setStatus('Укажи корректный Telegram ID');
            return;
        }

        const data: TelegramUserData = {
            telegramId: parsed,
            userName: null,
            firstName: `User ${parsed}`,
            lastName: null,
            languageCode: 'ru',
        };

        window.localStorage.setItem('ayanami.manualTelegramId', String(parsed));
        setTelegramIdInput(String(parsed));
        setMenuOpen(false);
        void bootstrapFromData(data);
    }

    async function bootstrapFromData(data: TelegramUserData, isCancelled?: () => boolean) {
        setLoading(true);
        setBootstrapError('');
        setBootstrapResponseBody('');
        setBootstrapStep(`checking:${data.telegramId}`);
        pushBootstrapTrace(`bootstrap start for telegramId=${data.telegramId}`);
        setStatus('Проверяю пользователя...');

        try {
            pushBootstrapTrace(`GET /api/users/${data.telegramId}`);
            const existingUser = await apiFetch(`/api/users/${data.telegramId}`);

            if (isCancelled?.()) {
                return;
            }

            setMyProfile(existingUser as UserDto);
            setTelegramIdInput(String(data.telegramId));
            setBootstrapped(true);
            setBootstrapStep('user-found');
            pushBootstrapTrace(`user found for telegramId=${data.telegramId}`);
            setStatus('Пользователь найден');
            return;
        } catch (error) {
            const message = error instanceof Error ? error.message : 'Ошибка проверки пользователя';
            setBootstrapError(message);
            pushBootstrapTrace(`lookup failed: ${message}`);

            if (isCancelled?.()) {
                return;
            }

            if (message.includes('not found') || message.includes('404')) {
                try {
                    setBootstrapStep('creating-user');
                    pushBootstrapTrace(`POST /api/users for telegramId=${data.telegramId}`);
                    await apiFetch('/api/users', {
                        method: 'POST',
                        body: JSON.stringify(data),
                    });

                    if (isCancelled?.()) {
                        return;
                    }

                    setStatus('Пользователь создан');
                    setTelegramIdInput(String(data.telegramId));
                    setBootstrapStep('loading-created-user');
                    pushBootstrapTrace(`user created, reloading telegramId=${data.telegramId}`);
                    await loadUserByTelegramId(String(data.telegramId));
                    setBootstrapped(true);
                    setBootstrapStep('done');
                    return;
                } catch (createError) {
                    const createMessage = createError instanceof Error ? createError.message : 'Ошибка создания пользователя';
                    setBootstrapError(createMessage);
                    pushBootstrapTrace(`create failed: ${createMessage}`);
                    setStatus(createMessage);
                    return;
                }
            }

            setStatus(message);
            return;
        } finally {
            if (!isCancelled?.()) {
                setLoading(false);
            }
        }
    }

    async function apiFetch(path: string, init?: RequestInit) {
        const authHeaders: Record<string, string> = {};

        console.log("DEBUG: window.Telegram exists:", !!window.Telegram);
        console.log("DEBUG: initData exists:", !!window.Telegram?.WebApp?.initData);

        if (enableTelegramMock) {
            const debugTelegramId = myProfile?.telegramId?.toString() ?? telegramIdInput.trim() ?? manualTelegramId.trim();
            if (debugTelegramId) {
                authHeaders['X-Debug-Telegram-Id'] = debugTelegramId;
            }
        } else if (window.Telegram?.WebApp?.initData) {
            authHeaders['X-Telegram-InitData'] = window.Telegram.WebApp.initData;
            console.log("DEBUG: Adding header X-Telegram-InitData");
        } else {
            console.warn("DEBUG: No InitData found! Headers will be empty.");
        }

        const response = await fetch(`${apiBaseUrl}${path}`, {
            headers: {
                'Content-Type': 'application/json',
                ...authHeaders,
                ...(init?.headers ?? {}),
            },
            ...init,
        });

        const text = await response.text();
        let body: unknown = text;
        try { body = JSON.parse(text); } catch { }

        setBootstrapResponseBody(typeof body === 'string' ? body : JSON.stringify(body, null, 2));

        if (!response.ok) {
            const errorMessage = typeof body === 'object' && body !== null && 'error' in body
                ? String((body as { error?: string }).error)
                : `HTTP ${response.status}`;

            setBootstrapError(errorMessage);
            pushBootstrapTrace(`${init?.method ?? 'GET'} ${path} -> ${response.status} ${errorMessage}`);
            throw new Error(errorMessage);
        }

        pushBootstrapTrace(`${init?.method ?? 'GET'} ${path} -> ${response.status}`);
        return body;
    }

    async function loadUserByTelegramId(telegramId: string) {
        try {
            setBootstrapStep(`loading-user:${telegramId}`);
            pushBootstrapTrace(`GET /api/users/${telegramId} (reload)`);
            const user = await apiFetch(`/api/users/${telegramId}`);
            setMyProfile(user as UserDto);
            await loadBalanceOperations(telegramId);
            setBootstrapStep('reloaded');
        } catch {
            setBootstrapError(`Failed to reload user ${telegramId}`);
            pushBootstrapTrace(`reload failed for telegramId=${telegramId}`);
        }
    }

    async function loadUser(targetTelegramId?: string) {
        const telegramId = (targetTelegramId ?? telegramIdInput).trim();

        if (!telegramId) {
            setStatus('Укажи Telegram ID');
            return;
        }

        setLoading(true);
        setStatus('Загружаю пользователя...');

        try {
            const user = await apiFetch(`/api/users/${telegramId}`);
            // Если мы обновляем себя — пишем в myProfile, если найденного юзера — в selectedUser
            if (myProfile && String(myProfile.telegramId) === telegramId) {
                setMyProfile(user as UserDto);
            } else {
                setSelectedUser(user as UserDto);
            }
            setTelegramIdInput(telegramId);
            await loadBalanceOperations(telegramId);
            setStatus('Пользователь загружен');
        } catch (error) {
            setStatus(error instanceof Error ? error.message : 'Ошибка загрузки');
        } finally {
            setLoading(false);
        }
    }

    async function loadAllUsers() {
        setLoading(true);
        setStatus('Загружаю пользователей...');

        try {
            const result = await apiFetch('/api/users');
            setUsers(Array.isArray(result) ? (result as UserDto[]) : []);
            setStatus('Список обновлён');
        } catch (error) {
            setStatus(error instanceof Error ? error.message : 'Ошибка загрузки списка');
        } finally {
            setLoading(false);
        }
    }

    async function searchAdminUser() {
        const query = adminSearchValue.trim();

        if (!query) {
            setStatus('Укажи Telegram ID, UUID или username');
            return;
        }

        setLoading(true);
        setStatus('Ищу пользователя...');

        try {
            // ИСПРАВЛЕН ЭНДПОИНТ И ИСПОЛЬЗУЕТСЯ СТРОГО setSelectedUser
            const user = await apiFetch(`/api/admin/users/${encodeURIComponent(query)}`);
            setSelectedUser(user as UserDto);
            setTelegramIdInput(String((user as UserDto).telegramId));
            await loadBalanceOperations(String((user as UserDto).telegramId));
            setExpandedAdminUserId((user as UserDto).id); // автоматически раскрываем его карточку
            setStatus('Пользователь найден');
            showToast('Пользователь найден', 'success');
        } catch (error) {
            const message = error instanceof Error ? error.message : 'Ошибка поиска';
            setStatus(message);
            showToast('Не удалось найти пользователя', 'error', message);
        } finally {
            setLoading(false);
        }
    }

    function selectAdminUser(user: UserDto) {
        setTelegramIdInput(String(user.telegramId));
        setSelectedUser(user); // ПИШЕМ В ИЗОЛИРОВАННОЕ СОСТОЯНИЕ ДЛЯ АДМИНКИ
        setStatus(`Выбран пользователь ${user.firstName}`);
        setExpandedAdminUserId(previous => (previous === user.id ? null : user.id));
    }

    function formatDateFromUnixMs(value: number) {
        const date = new Date(value);
        return Number.isNaN(date.getTime()) ? '—' : formatDate(date.toISOString());
    }

    async function addBalance(targetTelegramId?: string) {
        const telegramId = (targetTelegramId ?? telegramIdInput).trim();

        if (!telegramId) {
            setStatus('Укажи Telegram ID');
            return;
        }

        const amount = Number(balanceAmount);
        if (!Number.isFinite(amount) || amount <= 0) {
            setStatus('Укажи корректную сумму');
            return;
        }

        setLoading(true);
        setStatus('Пополняю баланс...');

        try {
            await apiFetch(`/api/users/${telegramId}/addToBalance/${amount}`, {
                method: 'POST',
            });
            setStatus('Баланс пополнен');
            showToast('Баланс пополнен', 'success', `${amount} ₽ зачислено на счёт`);
            await loadUser(telegramId);
            if (location.pathname === '/admin') {
                await loadAllUsers();
            }
        } catch (error) {
            const message = error instanceof Error ? error.message : 'Ошибка пополнения';
            setStatus(message);
            showToast('Не удалось пополнить баланс', 'error', message);
        } finally {
            setLoading(false);
        }
    }

    async function extendSubscription(plan: 'Trial' | 'Monthly' | 'Quarterly' | 'Yearly', targetTelegramId?: string) {
        const telegramId = (targetTelegramId ?? telegramIdInput).trim();

        if (!telegramId) {
            setStatus('Укажи Telegram ID');
            return;
        }

        setLoading(true);
        setStatus('Продлеваю подписку...');

        try {
            const updatedUser = await apiFetch(`/api/admin/${telegramId}/extend/${plan}`, {
                method: 'POST',
            });
            setStatus('Подписка продлена');
            showToast('Подписка выдана', 'success', `${plan} для ${telegramId}`);
            if (updatedUser && typeof updatedUser === 'object') {
                setSelectedUser(updatedUser as UserDto);
            }
            await loadUser(telegramId);
            if (location.pathname === '/admin') {
                await loadAllUsers();
            }
        } catch (error) {
            const message = error instanceof Error ? error.message : 'Ошибка продления';
            setStatus(message);
            showToast('Не удалось выдать подписку', 'error', message);
        } finally {
            setLoading(false);
        }
    }

    async function buySubscription(plan: Plan['name']) {
        const telegramId = (myProfile?.telegramId?.toString() || telegramIdInput.trim() || manualTelegramId.trim()).trim();

        if (!telegramId) {
            setStatus('Укажи Telegram ID');
            return;
        }

        const backendPlan = purchasePlanMap[plan];

        setLoading(true);
        setStatus('Покупаю подписку...');

        try {
            const updatedUser = await apiFetch(`/api/subscriptions/${telegramId}/extend/${backendPlan}`, {
                method: 'POST',
            });

            setStatus('Подписка куплена');
            showToast('Подписка оплачена!', 'success');
            if (updatedUser && typeof updatedUser === 'object') {
                setMyProfile(updatedUser as UserDto);
            }
            await loadUser(telegramId);

            if (location.pathname === '/admin') {
                await loadAllUsers();
            }
        } catch (error) {
            const message = error instanceof Error ? error.message : 'Ошибка покупки';
            setStatus(message);
            showToast('Не удалось оплатить подписку', 'error', message);
        } finally {
            setLoading(false);
        }
    }

    function formatSubscriptionPrice(subscription?: UserDto['subscriptions'][number] | null) {
        if (!subscription) {
            return '—';
        }

        if (subscription.name === 'Trial') {
            return '0 ₽';
        }

        return `${subscription.price} ₽`;
    }

    async function copyConnectionLink() {
        try {
            await navigator.clipboard.writeText(connectionLink);
            setStatus('Ссылка скопирована');
        } catch {
            setStatus('Не удалось скопировать ссылку');
        }
    }

    function openConnectionLink() {
        window.open(connectionLink, '_blank', 'noopener,noreferrer');
    }

    return (
        <div className={rootClassName}>
            <div className="ambient" aria-hidden="true">
                <span className="ambient__orb ambient__orb--one"/>
                <span className="ambient__orb ambient__orb--two"/>
                <span className="ambient__orb ambient__orb--three"/>
            </div>

            {menuOpen ? <div className="backdrop" onClick={() => setMenuOpen(false)} aria-hidden="true"/> : null}

            {showBootstrapDebug ? (
                <aside className={debugOpen ? 'debug-dock debug-dock--open' : 'debug-dock'}>
                    <button
                        className="debug-dock__toggle"
                        type="button"
                        onClick={() => setDebugOpen(value => !value)}
                        aria-label="Toggle bootstrap debug"
                    >
                        Debug
                    </button>

                    {debugOpen ? (
                        <div className="debug-dock__panel">
                            <div className="debug-dock__title">Bootstrap log</div>
                            <div className="debug-dock__row">
                                <span>Step</span>
                                <strong>{bootstrapStep}</strong>
                            </div>
                            <div className="debug-dock__row">
                                <span>Status</span>
                                <strong>{status}</strong>
                            </div>
                            <div className="debug-dock__row">
                                <span>Telegram ID</span>
                                <strong>{telegramIdInput.trim() || manualTelegramId.trim() || '—'}</strong>
                            </div>
                            <div className="debug-dock__row">
                                <span>Source</span>
                                <strong>{initTelegramData ? 'telegram' : 'manual'}</strong>
                            </div>
                            <div className="debug-dock__row">
                                <span>Error</span>
                                <strong>{bootstrapError || '—'}</strong>
                            </div>
                            <div className="debug-dock__trace">
                                {bootstrapTrace.length > 0
                                    ? bootstrapTrace.map((entry, index) => <span
                                        key={`${index}-${entry}`}>{entry}</span>)
                                    : <span>—</span>}
                            </div>
                            <button
                                className="debug-dock__copy"
                                type="button"
                                onClick={async () => {
                                    try {
                                        await navigator.clipboard.writeText(buildBootstrapDebugText());
                                        setStatus('Debug copied');
                                    } catch {
                                        setStatus('Copy failed');
                                    }
                                }}
                            >
                                Copy debug
                            </button>
                        </div>
                    ) : null}
                </aside>
            ) : null}

            <header className="topbar">
                <div className="brand-block">
                    <button
                        className="brand-mark"
                        type="button"
                        onClick={() => {
                            replayHomeReveal();
                            navigate('/');
                        }}
                        aria-label="На главную"
                    >
                        <img className="brand-mark__image" src="/ayanami-logo.svg" alt="" aria-hidden="true"/>
                    </button>
                    <div className="brand-copy">
                        <div className="brand">Ayanami Connect</div>
                        <div className="brand-subtitle">Стабильный VPN-сервис!</div>
                    </div>
                </div>

                <div className="topbar__actions">
                    <button
                        className="theme-toggle"
                        onClick={() => setTheme(theme === 'light' ? 'dark' : 'light')}
                        aria-label={theme === 'light' ? 'Включить тёмную тему' : 'Включить светлую тему'}
                        title={theme === 'light' ? 'Тёмная тема' : 'Светлая тема'}
                    >
                        {theme === 'light' ? '☾' : '☀'}
                    </button>

                    <button
                        className={menuOpen ? 'menu-toggle menu-toggle--open' : 'menu-toggle'}
                        onClick={() => setMenuOpen(value => !value)}
                        aria-label="Открыть меню"
                        aria-expanded={menuOpen}
                    >
                        <span/>
                        <span/>
                        <span/>
                    </button>
                </div>
            </header>

            <nav className={menuOpen ? 'nav nav--open' : 'nav'}>
                {navItems
                    .filter(item => item.path !== '/admin' || isAdmin)
                    .map(item => (
                        <button
                            key={item.path}
                            className={location.pathname === item.path ? 'nav__item nav__item--active' : 'nav__item'}
                            onClick={() => navigate(item.path)}
                        >
                            {item.title}
                        </button>
                    ))}
            </nav>

            {!bootstrapData ? (
                <div className="boot-overlay">
                    <div className="boot-card">
                        <div className="section-eyebrow">Внимание</div>
                        <h1>Профиль доступен только через Telegram</h1>
                        <p>Если останетесь здесь, то сможете только посмотреть на дизайн.</p>
                    </div>
                </div>
            ) : null}

            <main className="shell">
                <Routes>
                    <Route
                        path="/"
                        element={
                            <div key={`home-${homeRevealToken}`} className="page page--home-reveal page--route-reveal">
                                <section className="panel panel--feature panel--hero">
                                    <div className="panel-head panel-head--stack">
                                        <div className="hero-heading">
                                            <h1>Баланс</h1>
                                            <div className="hero-balance">{`${myProfile?.balance ?? 0} ₽`}</div>
                                            <p>
                                                Здесь отображается твой текущий баланс. Его можно использовать для
                                                продления подписки и оплаты.
                                            </p>
                                        </div>
                                    </div>
                                </section>

                                <div
                                    className={subscriptionFlipped ? 'subscription-solo subscription-solo--flipped' : 'subscription-solo'}>
                                    <div
                                        className="subscription-flip"
                                        role="button"
                                        tabIndex={0}
                                        aria-label="Переключить карточку подписки"
                                        onClick={() => setSubscriptionFlipped(value => !value)}
                                        onKeyDown={event => {
                                            if (event.key === 'Enter' || event.key === ' ') {
                                                event.preventDefault();
                                                setSubscriptionFlipped(value => !value);
                                            }
                                        }}
                                    >
                                        <div className="subscription-flip__inner">
                                            <div className="subscription-flip__face subscription-flip__face--front">
                                                <div className="hero-glass hero-glass--subscription">
                                                    <div className="hero-glass__label hero-glass__label--inline">
                                                        <span>ПОДПИСКА</span>
                                                        <span
                                                            className="hero-glass__label-note">Обновлено только что</span>
                                                    </div>
                                                    <div className="hero-glass__value">
                                                        {currentSubscription?.name ?? 'Trial'}
                                                        <span className="hero-glass__divider">|</span>
                                                        <span className="hero-glass__price-inline">
                              {formatSubscriptionPrice(currentSubscription)}
                            </span>
                                                    </div>
                                                    <div className="hero-glass__meta">
                                                        <span>{currentSubscription?.endedAt ? `До ${formatDate(currentSubscription.endedAt)}` : 'До 01.01.27'}</span>
                                                        <span>{currentSubscription?.id ? `ID ${currentSubscription.id}` : 'ID znfqvynhevcf4him'}</span>
                                                    </div>
                                                    <div className="hero-glass__meter">
                                                        <span style={{width: currentSubscription ? '82%' : '38%'}}/>
                                                    </div>
                                                </div>
                                            </div>

                                            <div className="subscription-flip__face subscription-flip__face--back">
                                                <div className="subscription-back">
                                                    <div className="subscription-back__header">
                                                        <div className="hero-glass__label">Подключение</div>
                                                        <div className="subscription-back__title">Ссылка для
                                                            подключения
                                                        </div>
                                                        <div className="subscription-back__link">{connectionLink}</div>
                                                    </div>

                                                    <div className="subscription-back__actions">
                                                        <button
                                                            className="primary-button"
                                                            type="button"
                                                            onClick={event => {
                                                                event.stopPropagation();
                                                                openConnectionLink();
                                                            }}
                                                        >
                                                            Открыть
                                                        </button>
                                                        <button
                                                            className="secondary-button"
                                                            type="button"
                                                            onClick={event => {
                                                                event.stopPropagation();
                                                                void copyConnectionLink();
                                                            }}
                                                        >
                                                            Скопировать
                                                        </button>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <section className="panel panel--glass panel--userinfo">
                                    <div className="panel-head">
                                        <div>
                                            <h2>Информация о пользователе</h2>
                                        </div>
                                    </div>

                                    <div className="data-grid data-grid--expanded user-card__grid--matrix">
                                        <DataField label="Telegram ID"
                                                   value={myProfile?.telegramId?.toString() ?? '—'}/>
                                        <DataField label="Username" value={myProfile?.userName ?? '—'}/>
                                        <DataField label="Роль" value={myProfile?.role ?? '—'}/>
                                    </div>
                                </section>
                            </div>
                        }
                    />

                    <Route
                        path="/connection"
                        element={
                            <div className="page page--route-reveal">
                                <section className="panel">
                                    <div className="panel-head">
                                        <div>
                                            <h2>Как подключиться</h2>
                                        </div>
                                    </div>

                                    <div className="step-grid">
                                        <StepCard
                                            index="01"
                                            title="Установи клиент"
                                            text="Подойдёт любой клиент, который поддерживает импорт подписки по ссылке."
                                        />
                                        <StepCard
                                            index="02"
                                            title="Открой подписку"
                                            text="На главной нажми на карточку подписки и открой ссылку подключения через кнопку Открыть."
                                        />
                                        <StepCard
                                            index="03"
                                            title="Обновляй доступ"
                                            text="После продления просто обнови подписку в клиенте, и срок подтянется автоматически."
                                        />
                                    </div>
                                </section>
                            </div>
                        }
                    />

                    <Route
                        path="/balance"
                        element={
                            <div className="page page--route-reveal">
                                <section className="panel panel--glass balance-hero">
                                    <div className="balance-hero__top">
                                        <div>
                                            <h2>Текущий баланс</h2>
                                            <p>Баланс используется для продления подписки и ручных операций внутри кабинета.</p>
                                        </div>
                                        <div className="balance-hero__amount">{`${myProfile?.balance ?? 0} ₽`}</div>
                                    </div>

                                    <div className="balance-hero__actions">
                                        <button
                                            className="primary-button"
                                            type="button"
                                            onClick={() => {
                                                showToast('Пополнение через @escal3de', 'info', 'Временно пополнение выполняется вручную');
                                                setStatus('Пополнение через @escal3de');
                                            }}
                                        >
                                            Пополнить
                                        </button>
                                        <button
                                            className="secondary-button"
                                            type="button"
                                            onClick={() => {
                                                const currentBalance = myProfile?.balance ?? 0;

                                                if (currentBalance < 3000) {
                                                    showToast('Вывод недоступен', 'error', 'Вывод разрешён только от 3 000 ₽');
                                                    setStatus('Вывод разрешён только от 3 000 ₽');
                                                    return;
                                                }

                                                showToast('Вывод доступен', 'success', 'Можно оформить заявку на вывод');
                                                setStatus('Вывод будет подключен позже');
                                            }}
                                        >
                                            Вывести
                                        </button>
                                    </div>
                                </section>

                                <section className="panel panel--glass balance-operations">
                                    <div className="panel-head">
                                        <div>
                                            <h2>Операции</h2>
                                            <p className="panel-subtitle">Последние движения средств с цветовой маркировкой по типам.</p>
                                        </div>
                                        <button
                                            className="icon-button"
                                            type="button"
                                            aria-label="Фильтр операций"
                                            onClick={() => {
                                                setBalanceFilterClosing(false);
                                                setBalanceFilterOpen(true);
                                            }}
                                        >
                                            <svg viewBox="0 0 24 24" aria-hidden="true">
                                                <path d="M3 5h18l-6.8 7.7V19l-4.4-1.9v-4.4L3 5Z"/>
                                            </svg>
                                        </button>
                                    </div>

                                    <div className="balance-operation-list">
                                        {filteredBalanceOperations.length === 0 ? (
                                            <div
                                                className="empty-state">{balanceFilter === 'all' ? 'Операций пока нет. Они появятся после пополнения, покупки или админских действий.' : `По фильтру «${activeBalanceFilter.label}» операций пока нет.`}</div>
                                        ) : (
                                            filteredBalanceOperations.map(operation => (
                                                <article key={operation.id}
                                                         className={`balance-operation balance-operation--${operation.kind}`}>
                                                    <div className="balance-operation__main">
                                                        <div
                                                            className="balance-operation__title">{operation.title}</div>
                                                        <div className="balance-operation__note">{operation.note}</div>
                                                    </div>
                                                    <div className="balance-operation__side">
                                                        <strong>{operation.amount}</strong>
                                                        <span>{operation.time}</span>
                                                    </div>
                                                </article>
                                            ))
                                        )}
                                    </div>
                                </section>
                            </div>
                        }
                    />

                    <Route
                        path="/payment"
                        element={
                            <div className="page page--route-reveal">
                                <section className="panel">
                                    <div className="panel-head">
                                        <div>
                                            <h2>Подписки и цены</h2>
                                        </div>
                                        <div className="section-note">Оплата осуществляется через @escal3de</div>
                                    </div>

                                    <div className="plan-grid">
                                        {plans.map(plan => (
                                            <article key={plan.name} className="plan-card">
                                                <div className="plan-card__header">
                                                    <div>
                                                        <div className="plan-card__title">{plan.title}</div>
                                                        <div className="plan-card__summary">{plan.summary}</div>
                                                    </div>
                                                    <div className="price">{plan.price}</div>
                                                </div>
                                                <button
                                                    className="secondary-button"
                                                    type="button"
                                                    onClick={() => void buySubscription(plan.name)}
                                                >
                                                    Купить
                                                </button>
                                            </article>
                                        ))}
                                    </div>
                                </section>
                            </div>
                        }
                    />

                    <Route
                        path="/admin"
                        element={
                            isAdmin ? (
                                <div className="page page--route-reveal">
                                    <section className="panel panel--glass admin-promos">
                                        <div className="panel-head">
                                            <div>
                                                <h2>Промокоды</h2>
                                                <p className="panel-subtitle">Интерфейс для будущих промокодов на баланс или дни подписки.</p>
                                            </div>
                                            <div className="section-note">Скоро</div>
                                        </div>

                                        <div className="promo-toggle">
                                            <button
                                                className={promoMode === 'days' ? 'promo-toggle__item promo-toggle__item--active' : 'promo-toggle__item'}
                                                type="button"
                                                onClick={() => setPromoMode('days')}
                                            >
                                                На дни
                                            </button>
                                            <button
                                                className={promoMode === 'balance' ? 'promo-toggle__item promo-toggle__item--active' : 'promo-toggle__item'}
                                                type="button"
                                                onClick={() => setPromoMode('balance')}
                                            >
                                                На баланс
                                            </button>
                                        </div>

                                        <div className="form-grid form-grid--promos">
                                            <label className="field">
                                                <span>Промокод</span>
                                                <input value={promoCodeValue}
                                                       onChange={e => setPromoCodeValue(e.target.value)}
                                                       placeholder="AYANAMI2026"/>
                                            </label>

                                            <label className="field">
                                                <span>{promoMode === 'days' ? 'Дни' : 'Баланс'}</span>
                                                <input value={promoGrantValue}
                                                       onChange={e => setPromoGrantValue(e.target.value)}
                                                       placeholder={promoMode === 'days' ? '7' : '179'}/>
                                            </label>
                                        </div>

                                        <div className="button-row button-row--single">
                                            <button
                                                className="primary-button"
                                                type="button"
                                                onClick={() => setStatus('Промокоды пока только в интерфейсе')}
                                            >
                                                Сохранить черновик
                                            </button>
                                        </div>
                                    </section>

                                    <section className="panel panel--glass admin-users">
                                        <div className="panel-head">
                                            <div>
                                                <h2>Пользователи</h2>
                                                <p className="panel-subtitle">Ищи по Telegram ID, UUID или username. Список подгружается сразу при входе.</p>
                                            </div>
                                            <div className="section-note">{users.length} шт.</div>
                                        </div>

                                        <div className="admin-search">
                                            <label className="field admin-search__field">
                                                <span>Поиск по Telegram ID / UUID / username</span>
                                                <input
                                                    value={adminSearchValue}
                                                    onChange={e => setAdminSearchValue(e.target.value)}
                                                    placeholder="1552110836, uuid или username"
                                                />
                                            </label>

                                            <button className="primary-button admin-search__button"
                                                    onClick={searchAdminUser} disabled={loading}>
                                                Найти
                                            </button>
                                        </div>

                                        <div className="users-list">
                                            {users.length === 0 ? (
                                                <div className="empty-state">Пока нет загруженных пользователей.</div>
                                            ) : (
                                                users.map(user => (
                                                    <article
                                                        key={user.id}
                                                        className={expandedAdminUserId === user.id ? 'user-row user-row--selectable user-row--expanded' : 'user-row user-row--selectable'}
                                                    >
                                                        <div
                                                            className="user-row__main user-row__main--toggle"
                                                            role="button"
                                                            tabIndex={0}
                                                            onClick={() => selectAdminUser(user)}
                                                            onKeyDown={event => {
                                                                if (event.key === 'Enter' || event.key === ' ') {
                                                                    event.preventDefault();
                                                                    selectAdminUser(user);
                                                                }
                                                            }}
                                                        >
                                                            <strong>{user.firstName}</strong>
                                                            <span>{user.userName ?? '—'}</span>
                                                        </div>
                                                        <div className="user-row__meta">
                                                            <span>{user.telegramId}</span>
                                                            <span>{user.balance} ₽</span>
                                                            <span>{user.subscriptions.length} подписок</span>
                                                            <span>{user.panelClients.length} клиентов</span>
                                                        </div>

                                                        {expandedAdminUserId === user.id ? (
                                                            <div className="user-row__details">
                                                                <div className="user-row__details-grid">
                                                                    <DataField label="ID пользователя" value={user.id}/>
                                                                    <DataField label="Telegram ID"
                                                                               value={user.telegramId.toString()}/>
                                                                    <DataField label="Username"
                                                                               value={user.userName ?? '—'}/>
                                                                    <DataField label="First name"
                                                                               value={user.firstName}/>
                                                                    <DataField label="Last name"
                                                                               value={user.lastName ?? '—'}/>
                                                                    <DataField label="Language"
                                                                               value={user.languageCode ?? '—'}/>
                                                                    <DataField label="Роль" value={user.role}/>
                                                                    <DataField label="Баланс"
                                                                               value={`${user.balance} ₽`}/>
                                                                    <DataField label="Создан"
                                                                               value={formatDate(user.createdAt)}/>
                                                                    <DataField label="Активность"
                                                                               value={formatDate(user.lastActiveAt)}/>
                                                                </div>

                                                                <div className="user-row__details-section">
                                                                    <div className="user-row__details-title">Подписки</div>
                                                                    {user.subscriptions.length > 0 ? (
                                                                        <div className="user-row__details-list">
                                                                            {user.subscriptions.map(subscription => (
                                                                                <div key={subscription.id}
                                                                                     className="user-row__details-item">
                                                                                    <strong>
                                                                                        {subscription.name}
                                                                                        {' | '}
                                                                                        {formatSubscriptionPrice(subscription)}
                                                                                    </strong>
                                                                                    <span>
                                          {subscription.status} · До {formatDate(subscription.endedAt)}
                                        </span>
                                                                                    <span>{subscription.email}</span>
                                                                                    <span>{subscription.plans}</span>
                                                                                </div>
                                                                            ))}
                                                                        </div>
                                                                    ) : (
                                                                        <div className="empty-state">Подписок пока нет.</div>
                                                                    )}
                                                                </div>

                                                                <div className="user-row__details-section">
                                                                    <div className="user-row__details-title">Клиенты 3X-UI</div>
                                                                    {user.panelClients.length > 0 ? (
                                                                        <div className="user-row__details-list">
                                                                            {user.panelClients.map(panelClient => (
                                                                                <div key={panelClient.id}
                                                                                     className="user-row__details-item">
                                                                                    <strong>{panelClient.email}</strong>
                                                                                    <span>UUID: {panelClient.uuid}</span>
                                                                                    <span>SubId: {panelClient.subId}</span>
                                                                                    <span>Expiry: {formatDateFromUnixMs(panelClient.expiryTime)}</span>
                                                                                    <span>{panelClient.enable ? 'Enabled' : 'Disabled'}</span>
                                                                                </div>
                                                                            ))}
                                                                        </div>
                                                                    ) : (
                                                                        <div className="empty-state">Клиентов пока нет.</div>
                                                                    )}
                                                                </div>

                                                                <div className="user-row__details-section">
                                                                    <div className="user-row__details-title">Управление</div>
                                                                    <div className="admin-user-actions">
                                                                        <div className="admin-user-actions__row">
                                                                            <label
                                                                                className="field admin-user-actions__amount-field">
                                                                                <span>Сумма</span>
                                                                                <input
                                                                                    value={balanceAmount}
                                                                                    onChange={event => setBalanceAmount(event.target.value)}
                                                                                    onMouseDown={event => event.stopPropagation()}
                                                                                    onPointerDown={event => event.stopPropagation()}
                                                                                    onClick={event => event.stopPropagation()}
                                                                                    inputMode="decimal"
                                                                                    placeholder="179"
                                                                                />
                                                                            </label>
                                                                            <button
                                                                                className="secondary-button"
                                                                                type="button"
                                                                                onMouseDown={event => event.stopPropagation()}
                                                                                onPointerDown={event => event.stopPropagation()}
                                                                                onClick={event => {
                                                                                    event.stopPropagation();
                                                                                    void addBalance(String(user.telegramId));
                                                                                }}
                                                                                disabled={loading}
                                                                            >
                                                                                Пополнить баланс
                                                                            </button>
                                                                        </div>

                                                                        <div className="admin-user-actions__plans">
                                                                            <label
                                                                                className="field admin-user-actions__plan-field">
                                                                                <span>Подписка</span>
                                                                                <select
                                                                                    value={adminSelectedPlan}
                                                                                    onChange={event => setAdminSelectedPlan(event.target.value as typeof adminSelectedPlan)}
                                                                                    onMouseDown={event => event.stopPropagation()}
                                                                                    onPointerDown={event => event.stopPropagation()}
                                                                                    onClick={event => event.stopPropagation()}
                                                                                >
                                                                                    {adminSubscriptionPlans.map(option => (
                                                                                        <option key={option.value}
                                                                                                value={option.value}>
                                                                                            {option.label}
                                                                                        </option>
                                                                                    ))}
                                                                                </select>
                                                                            </label>

                                                                            <button
                                                                                className="primary-button admin-user-actions__issue"
                                                                                type="button"
                                                                                onMouseDown={event => event.stopPropagation()}
                                                                                onPointerDown={event => event.stopPropagation()}
                                                                                onClick={event => {
                                                                                    event.stopPropagation();
                                                                                    void extendSubscription(adminSelectedPlan, String(user.telegramId));
                                                                                }}
                                                                                disabled={loading}
                                                                            >
                                                                                Выдать
                                                                            </button>
                                                                        </div>
                                                                    </div>
                                                                </div>
                                                            </div>
                                                        ) : null}
                                                    </article>
                                                ))
                                            )}
                                        </div>
                                    </section>
                                </div>
                            ) : (
                                <Navigate to="/" replace/>
                            )
                        }
                    />

                    <Route path="*" element={<Navigate to="/" replace/>}/>
                </Routes>
            </main>
            {(balanceFilterOpen || balanceFilterClosing) ? (
                <>
                    <button className="backdrop" type="button" aria-label="Закрыть меню фильтров"
                            onClick={closeBalanceFilterMenu}/>
                    <div
                        className={balanceFilterClosing ? 'nav nav--open balance-filter-menu balance-filter-menu--closing' : 'nav nav--open balance-filter-menu balance-filter-menu--open'}
                        role="dialog" aria-modal="true" aria-label="Фильтр операций">
                        {balanceFilterOptions.map(option => (
                            <button
                                key={option.value}
                                className={balanceFilter === option.value ? 'nav__item nav__item--active balance-filter-menu__item' : 'nav__item balance-filter-menu__item'}
                                type="button"
                                onClick={() => {
                                    setBalanceFilter(option.value);
                                    closeBalanceFilterMenu();
                                }}
                            >
                                <strong>{option.label}</strong>
                                <span>{option.description}</span>
                            </button>
                        ))}
                    </div>
                </>
            ) : null}

            {toast ? (
                <div className={`toast-host toast-host--${toast.kind}`} aria-live="polite" aria-atomic="true">
                    <div
                        className={`toast toast--${toast.kind} ${toast.closing ? 'toast--closing' : 'toast--opening'}`}>
                        <button
                            className="toast__close"
                            type="button"
                            aria-label="Закрыть уведомление"
                            onPointerDown={event => {
                                event.preventDefault();
                                event.stopPropagation();
                                dismissToast();
                            }}
                            onClick={dismissToast}
                        >
                            <span/>
                            <span/>
                        </button>
                        <div className="toast__copy">
                            <div className="toast__title">{toast.title}</div>
                            {toast.message ? <div className="toast__message">{toast.message}</div> : null}
                        </div>
                        <div className="toast__bar" key={toast.id}>
                            <span/>
                        </div>
                    </div>
                </div>
            ) : null}
        </div>
    );
}

function Section({title, description}: { title: string; description: string }) {
    return (
        <section className="section-head">
            <h1>{title}</h1>
            <p>{description}</p>
        </section>
    );
}

function StatTile({label, value}: { label: string; value: string }) {
    return (
        <div className="tile">
            <span>{label}</span>
            <strong>{value}</strong>
        </div>
    );
}

function DataField({label, value}: { label: string; value: string }) {
    return (
        <div className="field-card">
            <span>{label}</span>
            <strong>{value}</strong>
        </div>
    );
}

function StepCard({index, title, text}: { index: string; title: string; text: string }) {
    return (
        <article className="step-card">
            <div className="step-card__index">{index}</div>
            <strong>{title}</strong>
            <p>{text}</p>
        </article>
    );
}

function formatDate(value: string) {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) {
        return '—';
    }

    return date.toLocaleDateString('ru-RU');
}

function formatPrice(value?: number | null) {
    if (value == null) {
        return '—';
    }

    return `${value} ₽`;
}

function formatOperationAmount(value: number) {
    const formatted = Math.abs(value).toLocaleString('ru-RU');
    return value > 0 ? `+${formatted} ₽` : value < 0 ? `-${formatted} ₽` : '0 ₽';
}

function formatOperationTime(value: string | Date = new Date()) {
    const date = typeof value === 'string' ? new Date(value) : value;
    return new Intl.DateTimeFormat('ru-RU', {
        day: '2-digit',
        month: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
    }).format(date);
}

export default App;