/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: ["selector", '[data-theme="dark"]'],
  content: [
    "./templates/**/*.html",
    "./apps/**/templates/**/*.html",
    "./apps/**/*.py",
  ],
  theme: {
    extend: {
      colors: {
        brand: "var(--brand)",
        "brand-soft": "var(--brand-soft)",
        accent: "var(--accent)",
        danger: "var(--danger)",
        surface: "var(--bg)",
        "surface-soft": "var(--bg-soft)",
        "surface-translucent": "var(--bg-translucent)",
        card: "var(--card)",
        line: "var(--border)",
        ink: "var(--text)",
        "ink-muted": "var(--text-muted)",
        "ink-faint": "var(--text-faint)",
      },
      fontFamily: {
        sans: ["Vazirmatn", "ui-sans-serif", "system-ui", "sans-serif"],
      },
    },
  },
  plugins: [],
};
