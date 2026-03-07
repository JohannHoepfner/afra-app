import { useToast } from 'primevue';
import { mande, type MandeError } from 'mande';
import type { QuartalEnrollmentOverview } from '@/Profundum/models/feedback';
import type { ProfundumFachbereich, ProfundumSlot } from '@/Profundum/models/verwaltung';
import { gql } from '@/composables/graphql';

export const useManagement = () => {
    const toast = useToast();
    const api = mande('/api/profundum/management');

    async function getAllQuartaleWithEnrollments(): Promise<
        QuartalEnrollmentOverview[] | null
    > {
        try {
            return await api.get('/feedback/belegung');
        } catch (e) {
            const mandeError: MandeError = e;
            toast.add({
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Die Profunda konnten nicht geladen werden. Code ${mandeError.response.status}, ${mandeError.message}`,
            });
            return null;
        }
    }

    async function getFachbereiche(): Promise<ProfundumFachbereich[]> {
        try {
            const data = await gql<{ profundumFachbereiche: ProfundumFachbereich[] }>(`
                {
                    profundumFachbereiche {
                        id
                        label
                    }
                }
            `);
            return data.profundumFachbereiche;
        } catch (e) {
            toast.add({
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Die verfügbaren Fachbereiche der Profunda konnten nicht geladen werden.`,
            });
            return [];
        }
    }

    async function getSlots(): Promise<ProfundumSlot[]> {
        try {
            const data = await gql<{ profundumSlots: ProfundumSlot[] }>(`
                {
                    profundumSlots {
                        id
                        jahr
                        quartal
                        wochentag
                        einwahlZeitraum {
                            id
                        }
                    }
                }
            `);
            return data.profundumSlots.map((s) => ({
                ...s,
                einwahlZeitraumId: s.einwahlZeitraum?.id ?? '',
            }));
        } catch (e) {
            toast.add({
                summary: 'Es ist ein Fehler aufgetreten',
                detail: `Die verfügbaren Slots der Profunda konnten nicht geladen werden.`,
            });
            return [];
        }
    }

    return { getAllQuartaleWithEnrollments, getFachbereiche, getSlots };
};
