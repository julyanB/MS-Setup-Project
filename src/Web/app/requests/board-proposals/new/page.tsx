"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useEffect, useState } from "react";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { ApiError } from "@/lib/api";
import { boardProposalApi, storeBoardProposalId } from "@/lib/boardProposals";
import { DropDownOption, getDropDownOptions } from "@/lib/dropDowns";
import { useUser } from "@/contexts/UserContext";

export default function NewBoardProposalPage() {
  return (
    <ProtectedRoute>
      <NewBoardProposalContent />
    </ProtectedRoute>
  );
}

function NewBoardProposalContent() {
  const router = useRouter();
  const { user } = useUser();
  const [meetingDate, setMeetingDate] = useState(
    new Date().toISOString().slice(0, 16),
  );
  const [meetingType, setMeetingType] = useState("Regular");
  const [meetingFormat, setMeetingFormat] = useState("InPerson");
  const [secretaryEmployeeId, setSecretaryEmployeeId] = useState(
    user?.email ?? "",
  );
  const [responsibleBoardMemberEmployeeId, setResponsibleBoardMemberEmployeeId] =
    useState("");
  const [presenterEmployeeId, setPresenterEmployeeId] = useState(
    user?.email ?? "",
  );
  const [title, setTitle] = useState("Credit deal - Alfa OOD");
  const [category, setCategory] = useState("Business");
  const [description, setDescription] = useState(
    "Proposal prepared for board meeting review.",
  );
  const [meetingTypeOptions, setMeetingTypeOptions] = useState<DropDownOption[]>([]);
  const [meetingFormatOptions, setMeetingFormatOptions] = useState<DropDownOption[]>([]);
  const [categoryOptions, setCategoryOptions] = useState<DropDownOption[]>([]);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (user?.email) {
      setSecretaryEmployeeId((current) => current || user.email || "");
      setPresenterEmployeeId((current) => current || user.email || "");
    }
  }, [user?.email]);

  useEffect(() => {
    async function loadDropDowns() {
      const [meetingTypes, meetingFormats, categories] = await Promise.all([
        getDropDownOptions("MeetingType"),
        getDropDownOptions("MeetingFormat"),
        getDropDownOptions("Category"),
      ]);

      setMeetingTypeOptions(meetingTypes);
      setMeetingFormatOptions(meetingFormats);
      setCategoryOptions(categories);
      setMeetingType((current) => current || meetingTypes[0]?.code || "Regular");
      setMeetingFormat((current) => current || meetingFormats[0]?.code || "InPerson");
      setCategory((current) => current || categories[0]?.code || "Business");
    }

    void loadDropDowns();
  }, []);

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setSubmitting(true);
    setError(null);

    try {
      const id = await boardProposalApi.create({
        meetingDate: new Date(meetingDate).toISOString(),
        meetingType,
        meetingFormat,
        secretaryEmployeeId,
      });

      storeBoardProposalId(id);

      await boardProposalApi.nextStep(id, "Submit");
      await boardProposalApi.addAgendaItem(id, {
        title,
        initiatorEmployeeId: user?.email ?? secretaryEmployeeId,
        responsibleBoardMemberEmployeeId,
        presenterEmployeeId,
        category,
        description,
      });

      router.push(`/requests/board-proposals/${id}`);
    } catch (caught) {
      const message =
        caught instanceof ApiError ? caught.message : "Could not create request";
      setError(message);
    } finally {
      setSubmitting(false);
    }
  }

  return (
    <div className="space-y-6">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="text-xs font-semibold uppercase tracking-widest text-white/45">
            Board proposal request
          </p>
          <h1 className="display mt-2 text-3xl font-bold">Create proposal</h1>
          <p className="mt-1 max-w-2xl text-sm text-white/55">
            Creates the request, submits it into agenda preparation, and adds
            the first agenda item so the state machine can be tested.
          </p>
        </div>
        <Link href="/requests" className="btn-ghost">
          Back to requests
        </Link>
      </header>

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_360px]">
        <form onSubmit={submit} className="gls space-y-5 rounded-2xl p-6">
          {error && (
            <div className="rounded-2xl border border-rose-300/30 bg-rose-400/10 p-3 text-sm text-rose-100">
              {error}
            </div>
          )}

          <div className="grid gap-4 md:grid-cols-2">
            <div>
              <label className="label">Meeting date</label>
              <input
                type="datetime-local"
                className="field"
                value={meetingDate}
                onChange={(event) => setMeetingDate(event.target.value)}
                required
              />
            </div>
            <div>
              <label className="label">Secretary employee</label>
              <input
                className="field"
                value={secretaryEmployeeId}
                onChange={(event) => setSecretaryEmployeeId(event.target.value)}
                required
              />
            </div>
            <div>
              <label className="label">Meeting type</label>
              <select
                className="field"
                value={meetingType}
                onChange={(event) => setMeetingType(event.target.value)}
              >
                {meetingTypeOptions.map((option) => (
                  <option key={option.code} value={option.code}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="label">Meeting format</label>
              <select
                className="field"
                value={meetingFormat}
                onChange={(event) => setMeetingFormat(event.target.value)}
              >
                {meetingFormatOptions.map((option) => (
                  <option key={option.code} value={option.code}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="md:col-span-2">
              <label className="label">Agenda item title</label>
              <input
                className="field"
                value={title}
                onChange={(event) => setTitle(event.target.value)}
                maxLength={1000}
                required
              />
            </div>
            <div>
              <label className="label">Presenter employee</label>
              <input
                className="field"
                value={presenterEmployeeId}
                onChange={(event) => setPresenterEmployeeId(event.target.value)}
                required
              />
            </div>
            <div>
              <label className="label">Responsible board member</label>
              <input
                className="field"
                value={responsibleBoardMemberEmployeeId}
                onChange={(event) =>
                  setResponsibleBoardMemberEmployeeId(event.target.value)
                }
                required
              />
            </div>
            <div>
              <label className="label">Category</label>
              <select
                className="field"
                value={category}
                onChange={(event) => setCategory(event.target.value)}
              >
                {categoryOptions.map((option) => (
                  <option key={option.code} value={option.code}>
                    {option.label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <label className="label">Description</label>
            <textarea
              className="field min-h-32 resize-y"
              value={description}
              onChange={(event) => setDescription(event.target.value)}
              maxLength={1500}
            />
          </div>

          <button type="submit" className="btn-primary" disabled={submitting}>
            {submitting ? "Creating..." : "Create and open workflow"}
          </button>
        </form>

        <aside className="gls rounded-2xl p-5">
          <h2 className="display text-lg font-semibold">Test flow</h2>
          <div className="mt-4 space-y-3 text-sm text-white/65">
            <p>1. Create request as Draft.</p>
            <p>2. Submit moves it to AgendaPreparation.</p>
            <p>3. First agenda item is added automatically from this form.</p>
            <p>4. Continue the rest of the workflow on the details page.</p>
          </div>
        </aside>
      </div>
    </div>
  );
}
