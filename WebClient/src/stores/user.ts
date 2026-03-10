import { defineStore } from 'pinia';
import { mande } from 'mande';

export const useUser = defineStore('user', {
    state: () => ({
        loading: true,
        loggedIn: false,
        user: null,
    }),
    getters: {
        isStudent: (state) =>
            state.user.rolle === 'Oberstufe' || state.user.rolle === 'Mittelstufe',
        isMittelstufe: (state) => state.user.rolle === 'Mittelstufe',
        isTeacher: (state) => state.user.rolle === 'Tutor',
        isOtiumsverantwortlich: (state) =>
            state.user.berechtigungen.includes('Otiumsverantwortlich'),
        isProfundumsverantwortlich: (state) =>
            state.user.berechtigungen.includes('Profundumsverantwortlich'),
        isAdmin: (state) => state.user.berechtigungen.includes('Admin'),
        isImpersonating: (state) => state.user?.impersonationId != null,
    },
    actions: {
        async update() {
            const fetchUser = mande('/api/user');

            const userPromise = fetchUser.get();
            try {
                this.user = await userPromise;
                this.loggedIn = true;
            } catch (error) {
                if (error.response.status === 401) {
                    this.loggedIn = false;
                    this.user = null;
                    console.info('Not logged in');
                } else {
                    console.error('Error fetching user', error);
                    throw error;
                }
            } finally {
                this.loading = false;
            }
        },

        async logout() {
            // Navigate the browser to the logout endpoint so that OIDC end-session redirects
            // (if OIDC is enabled) are handled correctly. The server clears the local cookie and
            // then either redirects to Keycloak or returns 200.
            window.location.href = '/api/user/logout';
        },
    },
});
