import { Route, Routes } from "react-router-dom";
import Layout from "./components/Layout";
import { ProtectedRoute } from "./components/ProtectedRoute";

import Landing from "./pages/Landing";
import Search from "./pages/Search";
import AdvertDetail from "./pages/AdvertDetail";
import AdvertForm from "./pages/AdvertForm";
import MyAdverts from "./pages/MyAdverts";
import Profile from "./pages/Profile";
import PublicProfile from "./pages/PublicProfile";
import Chat from "./pages/Chat";
import Favorites from "./pages/Favorites";
import Notifications from "./pages/Notifications";
import Admin from "./pages/Admin";
import Moderator from "./pages/Moderator";

import Login from "./pages/Login";
import Register from "./pages/Register";
import ConfirmEmail from "./pages/ConfirmEmail";
import ForgotPassword from "./pages/ForgotPassword";
import ResetPassword from "./pages/ResetPassword";

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route index element={<Landing />} />
        <Route path="search" element={<Search />} />
        <Route path="adverts/:id" element={<AdvertDetail />} />
        <Route path="users/:id" element={<PublicProfile />} />

        <Route path="login" element={<Login />} />
        <Route path="register" element={<Register />} />
        <Route path="confirm-email" element={<ConfirmEmail />} />
        <Route path="forgot-password" element={<ForgotPassword />} />
        <Route path="reset-password" element={<ResetPassword />} />

        <Route element={<ProtectedRoute />}>
          <Route path="create-advert" element={<AdvertForm />} />
          <Route path="adverts/:id/edit" element={<AdvertForm />} />
          <Route path="my-adverts" element={<MyAdverts />} />
          <Route path="profile" element={<Profile />} />
          <Route path="chat" element={<Chat />} />
          <Route path="favorites" element={<Favorites />} />
          <Route path="notifications" element={<Notifications />} />
        </Route>

        <Route element={<ProtectedRoute roles={["Moderator", "Admin"]} />}>
          <Route path="moderator" element={<Moderator />} />
        </Route>

        <Route element={<ProtectedRoute roles={["Admin"]} />}>
          <Route path="admin" element={<Admin />} />
        </Route>
      </Route>
    </Routes>
  );
}
