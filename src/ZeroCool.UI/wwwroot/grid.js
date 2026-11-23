export function getRect(el) {
    const r = el.getBoundingClientRect();
    return { left: r.left, top: r.top, width: r.width, height: r.height };
}

export function capture(el, pointerId) {
    try { el.setPointerCapture(pointerId); } catch { /* ignore */ }
}

