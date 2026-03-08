import { useNotifications } from '@/stores/notifications';

/**
 * Handles browser Web Push subscription lifecycle.
 *
 * Call `subscribe()` once the user has consented to notifications.
 * The VAPID public key is fetched automatically from the backend.
 */
export function usePushNotifications() {
    const store = useNotifications();

    /** Returns true when Push API and Service Worker are available in this browser. */
    function isSupported() {
        return 'serviceWorker' in navigator && 'PushManager' in window;
    }

    /** Current browser notification permission: 'default' | 'granted' | 'denied' */
    function getPermission() {
        return Notification.permission;
    }

    /**
     * Request permission and subscribe to push notifications.
     * Returns the PushSubscription or null when the user denies permission or the
     * backend has no VAPID keys configured.
     */
    async function subscribe() {
        if (!isSupported()) return null;

        const publicKey = await store.getVapidPublicKey();
        if (!publicKey) return null;

        const permission = await Notification.requestPermission();
        if (permission !== 'granted') return null;

        const registration = await getServiceWorkerRegistration();
        const existing = await registration.pushManager.getSubscription();
        if (existing) {
            await store.savePushSubscription(existing);
            return existing;
        }

        const subscription = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey: urlBase64ToUint8Array(publicKey),
        });

        await store.savePushSubscription(subscription);
        return subscription;
    }

    /** Unsubscribe from push notifications and remove the subscription on the server. */
    async function unsubscribe() {
        if (!isSupported()) return;

        const registration = await getServiceWorkerRegistration();
        const subscription = await registration.pushManager.getSubscription();
        if (!subscription) return;

        await store.removePushSubscription(subscription);
        await subscription.unsubscribe();
    }

    return { isSupported, getPermission, subscribe, unsubscribe };
}

/** Converts a URL-safe base64 string to a Uint8Array (required by PushManager.subscribe). */
function urlBase64ToUint8Array(base64String) {
    const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
    const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
    const rawData = atob(base64);
    return Uint8Array.from([...rawData].map((c) => c.charCodeAt(0)));
}

/**
 * Returns the active ServiceWorkerRegistration, or rejects after `timeoutMs` ms.
 * navigator.serviceWorker.ready never settles when no service worker is registered
 * (e.g. in development mode), which would cause the Aktivieren button to hang forever.
 */
function getServiceWorkerRegistration(timeoutMs = 5000) {
    return Promise.race([
        navigator.serviceWorker.ready,
        new Promise((_, reject) =>
            setTimeout(
                () => reject(new Error('Service worker not ready within timeout')),
                timeoutMs,
            ),
        ),
    ]);
}
