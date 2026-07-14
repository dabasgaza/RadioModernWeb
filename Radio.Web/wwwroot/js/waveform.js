// ════════════════════════════════════════════════════════
//  Signal Console — animated transmission waveform
//  Vanilla, dependency-free. Respects prefers-reduced-motion.
// ════════════════════════════════════════════════════════
(function () {
    'use strict';

    const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    function setup(canvas) {
        const ctx = canvas.getContext('2d');
        let raf = null;
        let t = 0;

        function resize() {
            const dpr = window.devicePixelRatio || 1;
            const rect = canvas.getBoundingClientRect();
            canvas.width = Math.max(1, Math.floor(rect.width * dpr));
            canvas.height = Math.max(1, Math.floor(rect.height * dpr));
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
        }

        function color() {
            const v = getComputedStyle(canvas).color;
            return v && v !== 'rgba(0, 0, 0, 0)' ? v : getComputedStyle(document.documentElement).getPropertyValue('--primary').trim();
        }

        function draw() {
            const rect = canvas.getBoundingClientRect();
            const w = rect.width;
            const h = rect.height;
            const mid = h / 2;
            ctx.clearRect(0, 0, w, h);

            const stroke = color();
            const isLive = canvas.classList.contains('is-live');

            // Two layered waves with phase + slight noise for "signal" feel
            const layers = [
                { amp: h * 0.30, freq: 2.2, speed: 0.9, alpha: 0.55, width: 2 },
                { amp: h * 0.16, freq: 4.6, speed: 1.5, alpha: 0.30, width: 1.5 }
            ];

            for (const L of layers) {
                ctx.beginPath();
                for (let x = 0; x <= w; x += 2) {
                    const phase = (x / w) * Math.PI * 2 * L.freq + t * L.speed;
                    const env = 0.55 + 0.45 * Math.sin((x / w) * Math.PI); // taper edges
                    const noise = isLive ? Math.sin(phase * 3.1) * 0.06 : 0;
                    const y = mid + Math.sin(phase + noise) * L.amp * env;
                    if (x === 0) ctx.moveTo(x, y); else ctx.lineTo(x, y);
                }
                ctx.strokeStyle = stroke;
                ctx.globalAlpha = L.alpha;
                ctx.lineWidth = L.width;
                ctx.lineJoin = 'round';
                ctx.stroke();
            }
            ctx.globalAlpha = 1;

            if (!reduceMotion) {
                t += 0.02;
                raf = requestAnimationFrame(draw);
            }
        }

        resize();
        draw();
        if (!reduceMotion) {
            window.addEventListener('resize', resize);
        }
    }

    function init() {
        document.querySelectorAll('canvas.wf-waveform').forEach(setup);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
