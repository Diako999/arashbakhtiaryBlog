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
        accent: "var(--accent)",
      },
    },
  },
  plugins: [],
};
