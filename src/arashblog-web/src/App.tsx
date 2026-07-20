import { Navigate, Route, Routes } from "react-router-dom";
import Layout from "./components/Layout";
import DashboardLayout from "./components/DashboardLayout";
import PostList from "./pages/PostList";
import PostDetail from "./pages/PostDetail";
import Login from "./pages/Login";
import OtpSetup from "./pages/OtpSetup";
import OtpVerify from "./pages/OtpVerify";
import Overview from "./pages/dashboard/Overview";
import Analytics from "./pages/dashboard/Analytics";
import ContentList from "./pages/dashboard/ContentList";
import PostForm from "./pages/dashboard/PostForm";
import CommentModeration from "./pages/dashboard/CommentModeration";
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

      <Route path="/dashboard" element={<DashboardLayout />}>
        <Route index element={<Overview />} />
        <Route path="analytics" element={<Analytics />} />
        <Route path="content" element={<ContentList />} />
        <Route path="content/new" element={<PostForm />} />
        <Route path="content/:id/edit" element={<PostForm />} />
        <Route path="comments" element={<CommentModeration />} />
      </Route>

      <Route path="*" element={<Navigate to={`/${defaultLanguage}/blog`} replace />} />
    </Routes>
  );
}
