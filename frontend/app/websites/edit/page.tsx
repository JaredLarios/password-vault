"use client";

import { Suspense } from "react";
import { useSearchParams } from "next/navigation";
import UpdateWebsiteForm from "@/components/forms/UpdateWebsiteForm";

function EditWebsiteContent() {
  const searchParams = useSearchParams();
  const websiteId = searchParams.get("id");

  if (!websiteId) return <p>Missing website ID.</p>;

  return <UpdateWebsiteForm websiteId={websiteId} />;
}

export default function EditWebsitePage() {
  return (
    <Suspense fallback={<p>Loading...</p>}>
      <EditWebsiteContent />
    </Suspense>
  );
}