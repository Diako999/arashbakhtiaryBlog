import type { ReactNode } from "react";
import { useScrollReveal } from "../hooks/useScrollReveal";

// Fades/slides children in once scrolled into view. `delayMs` staggers
// siblings (landing sections, teaser grid cards) so they cascade in rather
// than popping simultaneously. The .reveal/.reveal-visible classes
// themselves are prefers-reduced-motion-gated in index.css.
export default function Reveal({ children, delayMs = 0 }: { children: ReactNode; delayMs?: number }) {
  const { ref, isVisible } = useScrollReveal<HTMLDivElement>();

  return (
    <div ref={ref} className={`reveal ${isVisible ? "reveal-visible" : ""}`} style={{ transitionDelay: `${delayMs}ms` }}>
      {children}
    </div>
  );
}
