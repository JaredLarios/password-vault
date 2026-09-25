import UpdateWebsiteForm from "@/components/forms/UpdateWebsiteForm";

export default function EditWebsitePage({ params }: { params: { id: string } }) {
  return <UpdateWebsiteForm websiteId={params.id} />;
}
