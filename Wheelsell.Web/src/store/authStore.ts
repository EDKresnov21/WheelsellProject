import { create } from "zustand";
import { persist } from "zustand/middleware";
import type { UserProfile } from "../types";

interface AuthState {
  accessToken: string | null;
  refreshToken: string | null;
  user: UserProfile | null;
  setAuth: (accessToken: string, refreshToken: string, user: UserProfile) => void;
  setUser: (user: UserProfile) => void;
  clear: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      accessToken: null,
      refreshToken: null,
      user: null,
      setAuth: (accessToken, refreshToken, user) => set({ accessToken, refreshToken, user }),
      setUser: (user) => set({ user }),
      clear: () => set({ accessToken: null, refreshToken: null, user: null })
    }),
    { name: "wheelsell-auth" }
  )
);
