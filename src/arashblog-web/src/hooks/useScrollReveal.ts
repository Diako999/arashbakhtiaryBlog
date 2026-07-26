import { useEffect, useRef, useState } from "react";

// Plain IntersectionObserver — no animation library is installed anywhere
// in this project, and a fade/slide-in-on-scroll effect doesn't need one.
// Unobserves after the first reveal so it never re-triggers on scroll-back.
export function useScrollReveal<T extends HTMLElement>() {
  const ref = useRef<T>(null);
  const [isVisible, setIsVisible] = useState(false);

  useEffect(() => {
    const el = ref.current;
    if (!el) return;

    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIsVisible(true);
          observer.unobserve(el);
        }
      },
      { threshold: 0.15 },
    );

    observer.observe(el);
    return () => observer.disconnect();
  }, []);

  return { ref, isVisible };
}
