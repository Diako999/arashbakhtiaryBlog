import { lazy, Suspense } from "react";
import { Navigate, Route, Routes } from "react-router-dom";
import { useTranslation } from "react-i18next";
import Layout from "./components/Layout";
import DashboardLayout from "./components/DashboardLayout";
import Home from "./pages/Home";
import PostList from "./pages/PostList";
import PostDetail from "./pages/PostDetail";
import OfferingList from "./pages/OfferingList";
import OfferingDetail from "./pages/OfferingDetail";
import TestimonialList from "./pages/TestimonialList";
import LeadList from "./pages/LeadList";
import LeadDetail from "./pages/LeadDetail";
import FlatPage from "./pages/FlatPage";
import Login from "./pages/Login";
import OtpSetup from "./pages/OtpSetup";
import OtpVerify from "./pages/OtpVerify";
import Overview from "./pages/dashboard/Overview";
import Analytics from "./pages/dashboard/Analytics";
import ContentList from "./pages/dashboard/ContentList";
import CommentModeration from "./pages/dashboard/CommentModeration";
import OfferingsList from "./pages/dashboard/OfferingsList";
import OfferingForm from "./pages/dashboard/OfferingForm";
import TestimonialsList from "./pages/dashboard/TestimonialsList";
import LeadsList from "./pages/dashboard/LeadsList";
import LeadForm from "./pages/dashboard/LeadForm";
import SubmissionInbox from "./pages/dashboard/SubmissionInbox";
import PagesVisibility from "./pages/dashboard/PagesVisibility";
import SettingsPage from "./pages/dashboard/SettingsPage";
import { defaultLanguage } from "./i18n";

// Code-split: PostForm pulls in the TipTap rich-text editor (~400kB), which
// public blog visitors never need — only dashboard authors editing a post do.
const PostForm = lazy(() => import("./pages/dashboard/PostForm"));

// The admin.* subdomain is meant to land admins straight on the dashboard
// (DashboardLayout's own gate then sends them to /dashboard/login if they
// aren't authenticated) instead of the public blog homepage. Everything
// else about the app is identical on that host — same SPA, same routes.
const rootPath = window.location.hostname.startsWith("admin.") ? "/dashboard" : `/${defaultLanguage}`;

export default function App() {
  const { t } = useTranslation();

  return (
    <Routes>
      <Route path="/" element={<Navigate to={rootPath} replace />} />
      <Route path="/:lang" element={<Layout />}>
        <Route index element={<Home />} />
        <Route path="blog" element={<PostList />} />
        <Route path="blog/:slug" element={<PostDetail />} />
        <Route path="offerings" element={<OfferingList />} />
        <Route path="offerings/:slug" element={<OfferingDetail />} />
        <Route path="testimonials" element={<TestimonialList />} />
        <Route path="free-resource" element={<LeadList />} />
        <Route path="free-resource/:slug" element={<LeadDetail />} />
        <Route path="about" element={<FlatPage slug="about" />} />
        <Route path="contact" element={<FlatPage slug="contact" />} />
      </Route>

      <Route path="/dashboard/login" element={<Login />} />
      <Route path="/dashboard/otp/setup" element={<OtpSetup />} />
      <Route path="/dashboard/otp/verify" element={<OtpVerify />} />

      <Route path="/dashboard" element={<DashboardLayout />}>
        <Route index element={<Overview />} />
        <Route path="analytics" element={<Analytics />} />
        <Route path="content" element={<ContentList />} />
        <Route
          path="content/new"
          element={
            <Suspense fallback={<p>{t("common.loading")}</p>}>
              <PostForm />
            </Suspense>
          }
        />
        <Route
          path="content/:id/edit"
          element={
            <Suspense fallback={<p>{t("common.loading")}</p>}>
              <PostForm />
            </Suspense>
          }
        />
        <Route path="comments" element={<CommentModeration />} />
        <Route path="offerings" element={<OfferingsList />} />
        <Route path="offerings/new" element={<OfferingForm />} />
        <Route path="offerings/:id/edit" element={<OfferingForm />} />
        <Route path="testimonials" element={<TestimonialsList />} />
        <Route path="leads" element={<LeadsList />} />
        <Route path="leads/new" element={<LeadForm />} />
        <Route path="leads/:id/edit" element={<LeadForm />} />
        <Route path="leads/submissions" element={<SubmissionInbox />} />
        <Route path="pages" element={<PagesVisibility />} />
        <Route path="settings" element={<SettingsPage />} />
      </Route>

      <Route path="*" element={<Navigate to={`/${defaultLanguage}`} replace />} />
    </Routes>
  );
}
