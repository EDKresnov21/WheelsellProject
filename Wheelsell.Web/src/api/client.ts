import axios from "axios";
import { useAuthStore } from "../store/authStore";

export const API_BASE_URL = "https://localhost:7100";

export const api = axios.create({
  baseURL: `${API_BASE_URL}/api`
});

api.interceptors.request.use((config) => {
  const token = useAuthStore.getState().accessToken;
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

let refreshPromise: Promise<string | null> | null = null;

async function refreshAccessToken(): Promise<string | null> {
  const { refreshToken, user, setAuth, clear } = useAuthStore.getState();
  if (!refreshToken) return null;

  try {
    const response = await axios.post(`${API_BASE_URL}/api/auth/refresh`, { refreshToken });
    const data = response.data as { accessToken: string; refreshToken: string; user: typeof user };
    if (data.user) {
      setAuth(data.accessToken, data.refreshToken, data.user);
    }
    return data.accessToken;
  } catch {
    clear();
    return null;
  }
}

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      if (!refreshPromise) {
        refreshPromise = refreshAccessToken().finally(() => {
          refreshPromise = null;
        });
      }

      const newToken = await refreshPromise;
      if (newToken) {
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return api(originalRequest);
      }
    }

    return Promise.reject(error);
  }
);

export function fileUrl(path?: string | null): string {
  if (!path) return "";
  return `${API_BASE_URL}${path}`;
}
