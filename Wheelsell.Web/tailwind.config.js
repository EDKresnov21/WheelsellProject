/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{js,ts,jsx,tsx}"],
  theme: {
    extend: {
      colors: {
        base: {
          950: "#0B0C0E",
          900: "#121317",
          800: "#1A1C22",
          700: "#262932",
          600: "#363B47",
          500: "#4B515F"
        },
        ink: {
          100: "#F4F5F7",
          300: "#C7CBD4",
          500: "#8B92A3"
        },
        amber: {
          400: "#F2B84B",
          500: "#E8A227",
          600: "#C8841A"
        }
      },
      fontFamily: {
        display: ["Sora", "sans-serif"],
        body: ["Inter", "sans-serif"]
      },
      borderRadius: {
        xl: "0.875rem"
      }
    }
  },
  plugins: []
};
