import { Navigate, Route, Routes } from "react-router-dom";
import Layout from "./components/Layout";
import PostList from "./pages/PostList";
import PostDetail from "./pages/PostDetail";
import Login from "./pages/Login";
import OtpSetup from "./pages/OtpSetup";
import OtpVerify from "./pages/OtpVerify";
import DashboardPlaceholder from "./pages/DashboardPlaceholder";
import { defaultLanguage } from "./i18n";

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<Navigate to={`/${defaultLanguage}/blog`} replace />} />
      <Route path="/:lang" element={<Layout />}>
        <Route index element={<Navigate to="blog" replace />} />
        <Route path="blog" element={<PostList />} />
        <Route path="blog/:slug" element={<PostDetail />} />
      </Route>
      <Route path="/dashboard/login" element={<Login />} />
      <Route path="/dashboard/otp/setup" element={<OtpSetup />} />
      <Route path="/dashboard/otp/verify" element={<OtpVerify />} />
      <Route path="/dashboard" element={<DashboardPlaceholder />} />
      <Route path="*" element={<Navigate to={`/${defaultLanguage}/blog`} replace />} />
    </Routes>
  );
}
