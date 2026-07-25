import useTheme from "../hooks/useTheme";

// 44x44 (not the mockup's literal 36x36) — meets the standard minimum
// touch-target size for accessibility.
export default function ThemeToggleButton() {
  const { theme, toggle } = useTheme();

  return (
    <button
      type="button"
      onClick={toggle}
      aria-label={theme === "dark" ? "Switch to light mode" : "Switch to dark mode"}
      className="flex h-11 w-11 shrink-0 items-center justify-center rounded-full border border-line bg-card transition-[transform,border-color] duration-300 ease-out hover:rotate-[20deg] hover:scale-105 hover:border-brand"
    >
      {theme === "dark" ? (
        <svg viewBox="0 0 24 24" width={20} height={20} fill="none" stroke="currentColor" strokeWidth={2}>
          <circle cx="12" cy="12" r="5" />
          <path d="M12 1v2M12 21v2M4.22 4.22l1.42 1.42M18.36 18.36l1.42 1.42M1 12h2M21 12h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42" />
        </svg>
      ) : (
        <svg viewBox="0 0 24 24" width={20} height={20} fill="currentColor">
          <path d="M21 12.79A9 9 0 1111.21 3a7 7 0 009.79 9.79z" />
        </svg>
      )}
    </button>
  );
}
