<script setup>
import { computed } from 'vue';

const props = defineProps({
    termine: {
        type: Array,
        default: () => [],
    },
});

// Build time-series: group by date, average attendance per day
const chartData = computed(() => {
    const relevant = props.termine.filter(
        (t) => t.durchschnittlicheAnwesenheit != null && !t.istAbgesagt,
    );
    if (relevant.length === 0) return [];

    const byDate = new Map();
    for (const t of relevant) {
        const d = t.datum;
        if (!byDate.has(d)) byDate.set(d, []);
        byDate.get(d).push(t.durchschnittlicheAnwesenheit);
    }

    return [...byDate.entries()]
        .map(([date, rates]) => ({
            date,
            rate: rates.reduce((a, b) => a + b, 0) / rates.length,
        }))
        .sort((a, b) => a.date.localeCompare(b.date));
});

const hasData = computed(() => chartData.value.length >= 1);

// SVG layout constants
const W = 560;
const H = 180;
const padLeft = 38;
const padRight = 16;
const padTop = 12;
const padBottom = 40;
const plotW = W - padLeft - padRight;
const plotH = H - padTop - padBottom;

const xPos = (i) => {
    const n = chartData.value.length;
    if (n <= 1) return padLeft + plotW / 2;
    return padLeft + (i / (n - 1)) * plotW;
};

const yPos = (val) => padTop + plotH - (val / 100) * plotH;

const points = computed(() =>
    chartData.value.map((d, i) => ({
        ...d,
        cx: xPos(i),
        cy: yPos(d.rate),
    })),
);

const linePath = computed(() => {
    if (points.value.length < 2) return '';
    return points.value.map((p, i) => `${i === 0 ? 'M' : 'L'} ${p.cx} ${p.cy}`).join(' ');
});

// For a single point render a short horizontal stroke so the gradient area fills
const areaPath = computed(() => {
    if (points.value.length === 0) return '';
    const bottom = padTop + plotH;
    if (points.value.length === 1) {
        const p = points.value[0];
        const half = 10;
        return `M ${p.cx - half} ${bottom} L ${p.cx - half} ${p.cy} L ${p.cx + half} ${p.cy} L ${p.cx + half} ${bottom} Z`;
    }
    const start = `M ${points.value[0].cx} ${bottom}`;
    const line = points.value.map((p) => `L ${p.cx} ${p.cy}`).join(' ');
    const end = `L ${points.value[points.value.length - 1].cx} ${bottom} Z`;
    return `${start} ${line} ${end}`;
});

const yTicks = [0, 25, 50, 75, 100];

const xTickIndices = computed(() => {
    const n = chartData.value.length;
    if (n <= 6) return chartData.value.map((_, i) => i);
    const step = Math.ceil(n / 6);
    const indices = [];
    for (let i = 0; i < n; i += step) indices.push(i);
    if (indices[indices.length - 1] !== n - 1) indices.push(n - 1);
    return indices;
});

function formatShortDate(dateStr) {
    // dateStr is DateOnly from C# serialized as "YYYY-MM-DD"; take only the date part
    const d = new Date(String(dateStr).slice(0, 10) + 'T00:00:00');
    return d.toLocaleDateString('de-DE', { day: '2-digit', month: 'short' });
}
</script>

<template>
    <div v-if="hasData" class="overflow-hidden">
        <svg
            :viewBox="`0 0 ${W} ${H}`"
            xmlns="http://www.w3.org/2000/svg"
            style="width: 100%; max-height: 220px"
        >
            <defs>
                <linearGradient id="afra-area-grad" x1="0" y1="0" x2="0" y2="1">
                    <stop offset="0%" stop-color="var(--p-primary-color)" stop-opacity="0.22" />
                    <stop offset="100%" stop-color="var(--p-primary-color)" stop-opacity="0.02" />
                </linearGradient>
            </defs>

            <!-- Horizontal grid lines -->
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

            <!-- Y-axis labels -->
            <text
                v-for="tick in yTicks"
                :key="tick"
                :x="padLeft - 5"
                :y="yPos(tick) + 4"
                text-anchor="end"
                font-size="11"
                fill="var(--p-text-muted-color)"
            >{{ tick }}%</text>

            <!-- Area fill -->
            <path v-if="areaPath" :d="areaPath" fill="url(#afra-area-grad)" />

            <!-- Line (for ≥2 points) -->
            <path
                v-if="linePath"
                :d="linePath"
                fill="none"
                stroke="var(--p-primary-color)"
                stroke-width="2"
                stroke-linejoin="round"
                stroke-linecap="round"
            />

            <!-- Data point dots -->
            <circle
                v-for="(p, i) in points"
                :key="i"
                :cx="p.cx"
                :cy="p.cy"
                r="3.5"
                fill="var(--p-primary-color)"
                stroke="var(--p-surface-0)"
                stroke-width="1.5"
            />

            <!-- X-axis date labels -->
            <text
                v-for="i in xTickIndices"
                :key="i"
                :x="xPos(i)"
                :y="padTop + plotH + 16"
                text-anchor="middle"
                font-size="10"
                fill="var(--p-text-muted-color)"
            >{{ formatShortDate(chartData[i].date) }}</text>
        </svg>
    </div>
    <p v-else class="text-surface-400 text-sm italic">Noch keine Anwesenheitsdaten vorhanden.</p>
</template>
