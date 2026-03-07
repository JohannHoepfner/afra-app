<script setup>
import { computed } from 'vue';

const props = defineProps({
    /** Array of { otiumId, bezeichnung, dataPoints: [{ datum, auslastung }] } */
    series: {
        type: Array,
        default: () => [],
    },
});

// SVG layout constants
const W = 640;
const H = 220;
const padLeft = 42;
const padRight = 16;
const padTop = 12;
const padBottom = 48;
const plotW = W - padLeft - padRight;
const plotH = H - padTop - padBottom;

// Assign a distinct color from a small palette
const COLORS = [
    'var(--p-primary-color)',
    '#e67e22',
    '#27ae60',
    '#8e44ad',
    '#c0392b',
    '#2980b9',
    '#f39c12',
    '#16a085',
    '#d35400',
    '#7f8c8d',
];

const seriesColor = (i) => COLORS[i % COLORS.length];

// Collect all data points across all series for axis computation
const allPoints = computed(() =>
    props.series.flatMap((s) =>
        s.dataPoints.map((d) => ({ datum: d.datum, auslastung: d.auslastung })),
    ),
);

const hasData = computed(() => allPoints.value.length > 0);

// Sorted unique dates across all series (for shared x-axis scale)
const allDates = computed(() => {
    const dates = [...new Set(allPoints.value.map((d) => d.datum))].sort((a, b) =>
        a.localeCompare(b),
    );
    return dates;
});

// Y-axis: always 0–100 (percentage) with 25-unit ticks
const yMax = 100;
const yTicks = [0, 25, 50, 75, 100];

const xPos = (dateStr) => {
    const n = allDates.value.length;
    if (n <= 1) return padLeft + plotW / 2;
    const i = allDates.value.indexOf(dateStr);
    return padLeft + (i / (n - 1)) * plotW;
};

const yPos = (val) => padTop + plotH - (val / yMax) * plotH;

// Build plotted points per series (only dates present in that series)
const plottedSeries = computed(() =>
    props.series.map((s, si) => ({
        bezeichnung: s.bezeichnung,
        color: seriesColor(si),
        points: s.dataPoints
            .slice()
            .sort((a, b) => a.datum.localeCompare(b.datum))
            .map((d) => ({
                datum: d.datum,
                auslastung: d.auslastung,
                cx: xPos(d.datum),
                cy: yPos(d.auslastung),
            })),
    })),
);

function linePath(pts) {
    if (pts.length < 2) return '';
    return pts.map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.cx} ${p.cy}`).join(' ');
}

// X-axis tick indices (at most ~8 labels)
const xTickIndices = computed(() => {
    const n = allDates.value.length;
    if (n <= 8) return allDates.value.map((_, i) => i);
    const step = Math.ceil(n / 8);
    const indices = [];
    for (let i = 0; i < n; i += step) indices.push(i);
    if (indices[indices.length - 1] !== n - 1) indices.push(n - 1);
    return indices;
});

function formatShortDate(dateStr) {
    const d = new Date(String(dateStr).slice(0, 10) + 'T00:00:00');
    return d.toLocaleDateString('de-DE', { day: '2-digit', month: 'short' });
}
</script>

<template>
    <div v-if="hasData" class="overflow-hidden">
        <svg
            :viewBox="`0 0 ${W} ${H}`"
            xmlns="http://www.w3.org/2000/svg"
            style="width: 100%; max-height: 280px"
        >
            <!-- Horizontal grid lines + Y-axis labels -->
            <line
                v-for="tick in yTicks"
                :key="tick"
                :x1="padLeft"
                :y1="yPos(tick)"
                :x2="padLeft + plotW"
                :y2="yPos(tick)"
                stroke="var(--p-surface-200)"
                stroke-width="1"
            />
            <text
                v-for="tick in yTicks"
                :key="tick"
                :x="padLeft - 5"
                :y="yPos(tick) + 4"
                text-anchor="end"
                font-size="11"
                fill="var(--p-text-muted-color)"
            >{{ tick }}%</text>

            <!-- One line + dots per series -->
            <g v-for="(s, si) in plottedSeries" :key="si">
                <!-- Line -->
                <path
                    v-if="linePath(s.points)"
                    :d="linePath(s.points)"
                    fill="none"
                    :stroke="s.color"
                    stroke-width="2"
                    stroke-linejoin="round"
                    stroke-linecap="round"
                />
                <!-- Dots -->
                <circle
                    v-for="(p, pi) in s.points"
                    :key="pi"
                    :cx="p.cx"
                    :cy="p.cy"
                    r="3.5"
                    :fill="s.color"
                    stroke="var(--p-surface-0)"
                    stroke-width="1.5"
                >
                    <title>{{ s.bezeichnung }}: {{ formatShortDate(p.datum) }} – {{ Math.round(p.auslastung) }}%</title>
                </circle>
            </g>

            <!-- X-axis date labels -->
            <text
                v-for="i in xTickIndices"
                :key="i"
                :x="xPos(allDates[i])"
                :y="padTop + plotH + 16"
                text-anchor="middle"
                font-size="10"
                fill="var(--p-text-muted-color)"
            >{{ formatShortDate(allDates[i]) }}</text>
        </svg>

        <!-- Legend -->
        <div class="flex flex-wrap gap-x-5 gap-y-1 mt-1">
            <span
                v-for="(s, si) in plottedSeries"
                :key="si"
                class="inline-flex items-center gap-1.5 text-sm"
            >
                <svg width="18" height="4" xmlns="http://www.w3.org/2000/svg">
                    <line x1="0" y1="2" x2="18" y2="2" :stroke="seriesColor(si)" stroke-width="2.5" />
                </svg>
                {{ s.bezeichnung }}
            </span>
        </div>
    </div>
    <p v-else class="text-surface-400 text-sm italic">Noch keine Anwesenheitsdaten vorhanden.</p>
</template>
