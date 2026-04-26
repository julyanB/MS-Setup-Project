"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { FormEvent, useEffect, useState } from "react";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { ApiError } from "@/lib/api";
import { boardProposalApi, storeBoardProposalId } from "@/lib/boardProposals";
import { DropDownOption, getDropDownOptions } from "@/lib/dropDowns";
import {
  employeeApi,
  employeeLabel,
  type EmployeeLookupItem,
} from "@/lib/employees";

const BOARD_PROPOSAL_SECRETARY_ROLE = "BoardProposalSecretaryAdmin";

export default function NewBoardProposalPage() {
  return (
    <ProtectedRoute>
      <NewBoardProposalContent />
    </ProtectedRoute>
  );
}

function NewBoardProposalContent() {
  const router = useRouter();
  const [meetingDate, setMeetingDate] = useState(
    new Date().toISOString().slice(0, 16),
  );
  const [meetingType, setMeetingType] = useState("Regular");
  const [meetingFormat, setMeetingFormat] = useState("InPerson");
  const [secretaryEmployeeId, setSecretaryEmployeeId] = useState("");
  const [meetingTypeOptions, setMeetingTypeOptions] = useState<DropDownOption[]>([]);
  const [meetingFormatOptions, setMeetingFormatOptions] = useState<DropDownOption[]>([]);
  const [secretaryEmployees, setSecretaryEmployees] = useState<EmployeeLookupItem[]>([]);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadSecretaries() {
      const employees = await employeeApi.lookup({
        role: BOARD_PROPOSAL_SECRETARY_ROLE,
        limit: 250,
      });

      setSecretaryEmployees(employees);
      setSecretaryEmployeeId((current) =>
        employees.some((employee) => employee.id === current)
          ? current
          : employees[0]?.id ?? "",
      );
    }

    void loadSecretaries();
  }, []);

  useEffect(() => {
    async function loadDropDowns() {
      const [meetingTypes, meetingFormats] = await Promise.all([
        getDropDownOptions("MeetingType"),
        getDropDownOptions("MeetingFormat"),
      ]);

      setMeetingTypeOptions(meetingTypes);
      setMeetingFormatOptions(meetingFormats);
      setMeetingType((current) => current || meetingTypes[0]?.code || "Regular");
      setMeetingFormat((current) => current || meetingFormats[0]?.code || "InPerson");
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
            Creates the meeting request as Draft. Agenda points and materials
            are added from the request workspace after creation.
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
              <select
                className="field"
                value={secretaryEmployeeId}
                onChange={(event) => setSecretaryEmployeeId(event.target.value)}
                required
              >
                <option value="" disabled>
                  Select secretary
                </option>
                {secretaryEmployees.map((employee) => (
                  <option key={employee.id} value={employee.id}>
                    {employeeLabel(employee)}
                  </option>
                ))}
              </select>
              {secretaryEmployees.length === 0 && (
                <p className="mt-2 text-xs text-amber-100/80">
                  No users have the BoardProposalSecretaryAdmin role. Assign it
                  from the admin panel first.
                </p>
              )}
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
          </div>

          <button type="submit" className="btn-primary" disabled={submitting}>
            {submitting ? "Creating..." : "Create meeting request"}
          </button>
        </form>

        <aside className="gls rounded-2xl p-5">
          <h2 className="display text-lg font-semibold">Flow</h2>
          <div className="mt-4 space-y-3 text-sm text-white/65">
            <p>1. Create the meeting as Draft.</p>
            <p>2. Add agenda points and upload materials.</p>
            <p>3. Preview readiness, then send the agenda.</p>
            <p>4. Mark held, register decisions and tasks, then monitor deadlines.</p>
          </div>
        </aside>
      </div>
    </div>
  );
}
