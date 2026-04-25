"use client";

import Link from "next/link";
import { useParams } from "next/navigation";
import {
  DragEvent,
  FormEvent,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import { ProtectedRoute } from "@/components/ProtectedRoute";
import { useUser } from "@/contexts/UserContext";
import { ApiError, apiDownload, apiFetch } from "@/lib/api";
import {
  BoardProposalRequestDetails,
  EmployeeRequestAction,
  boardProposalApi,
} from "@/lib/boardProposals";
import {
  DropDownOption,
  getDropDownLabel,
  getDropDownOptions,
} from "@/lib/dropDowns";

type Stage = {
  title: string;
  description: string;
  statuses: BoardProposalRequestDetails["status"][];
};

const stages: Stage[] = [
  {
    title: "Initiation",
    description: "Meeting and first agenda item are prepared.",
    statuses: ["Draft", "AgendaPreparation"],
  },
  {
    title: "Package",
    description: "Materials are uploaded and the package is ready.",
    statuses: ["MaterialsPreparation", "ReadyForReview"],
  },
  {
    title: "Meeting",
    description: "Agenda package is sent and the meeting is held.",
    statuses: ["Sent", "Held"],
  },
  {
    title: "Decision & tasks",
    description: "Votes, decision, and follow-up tasks are registered.",
    statuses: ["DecisionsRegistration", "TaskMonitoring"],
  },
  {
    title: "Archive",
    description: "Request is closed and read-only.",
    statuses: ["Closed", "Cancelled"],
  },
];

const nextActionByStatus: Partial<
  Record<
    BoardProposalRequestDetails["status"],
    { action: EmployeeRequestAction; label: string; note: string }
  >
> = {
  AgendaPreparation: {
    action: "MoveNext",
    label: "Move to materials",
    note: "Agenda is complete; start packaging files.",
  },
  MaterialsPreparation: {
    action: "MoveNext",
    label: "Package ready",
    note: "Materials are prepared for review.",
  },
  ReadyForReview: {
    action: "Send",
    label: "Mark package sent",
    note: "Marks the agenda/materials package as sent.",
  },
  Sent: {
    action: "MarkHeld",
    label: "Mark meeting held",
    note: "Opens vote, decision, and task registration.",
  },
  Held: {
    action: "RegisterDecisions",
    label: "Start decision registration",
    note: "Move into decision and vote capture.",
  },
  DecisionsRegistration: {
    action: "StartMonitoring",
    label: "Start monitoring",
    note: "Requires a registered decision.",
  },
  TaskMonitoring: {
    action: "Close",
    label: "Close request",
    note: "Requires at least one follow-up task.",
  },
  Closed: {
    action: "Reopen",
    label: "Reopen monitoring",
    note: "Returns the request to task monitoring.",
  },
};

export default function BoardProposalDetailsPage() {
  return (
    <ProtectedRoute>
      <BoardProposalDetailsContent />
    </ProtectedRoute>
  );
}

function BoardProposalDetailsContent() {
  const params = useParams<{ id: string }>();
  const id = Number(params.id);
  const { user } = useUser();
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const [details, setDetails] = useState<BoardProposalRequestDetails | null>(
    null,
  );
  const [loading, setLoading] = useState(true);
  const [working, setWorking] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [file, setFile] = useState<File | null>(null);
  const [draggedTaskId, setDraggedTaskId] = useState<number | null>(null);
  const [categoryOptions, setCategoryOptions] = useState<DropDownOption[]>([]);
  const [voteTypeOptions, setVoteTypeOptions] = useState<DropDownOption[]>([]);
  const [decisionStatusOptions, setDecisionStatusOptions] = useState<
    DropDownOption[]
  >([]);
  const [taskStatusOptions, setTaskStatusOptions] = useState<DropDownOption[]>(
    [],
  );
  const [documentTypeOptions, setDocumentTypeOptions] = useState<
    DropDownOption[]
  >([]);
  const [voteForm, setVoteForm] = useState({
    boardMemberEmployeeId: "",
    voteType: "Positive",
    notes: "",
  });
  const [decisionForm, setDecisionForm] = useState({
    decisionStatus: "Approved",
    decisionText: "",
    finalVote: "",
    notes: "",
  });
  const [taskForm, setTaskForm] = useState({
    title: "",
    description: "",
    responsibleEmployeeId: "",
    dueDate: new Date(Date.now() + 7 * 86400000).toISOString().slice(0, 16),
    status: "ToDo",
  });

  const firstAgendaItem = details?.agendaItems[0];
  const attachments = details?.attachments ?? [];
  const votes = firstAgendaItem?.votes ?? [];
  const tasks = useMemo(
    () =>
      [...(firstAgendaItem?.tasks ?? [])].sort(
        (a, b) =>
          (a.order ?? Number.MAX_SAFE_INTEGER) -
            (b.order ?? Number.MAX_SAFE_INTEGER) ||
          new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime(),
      ),
    [firstAgendaItem?.tasks],
  );
  const nextAction = details ? nextActionByStatus[details.status] : undefined;
  const currentStageIndex = details
    ? Math.max(
        0,
        stages.findIndex((stage) => stage.statuses.includes(details.status)),
      )
    : 0;

  const canCancel =
    details !== null &&
    details.status !== "Closed" &&
    details.status !== "Cancelled";

  const readiness = useMemo(
    () => [
      { label: "Agenda item exists", done: Boolean(firstAgendaItem) },
      { label: "At least one material uploaded", done: attachments.length > 0 },
      { label: "At least one vote captured", done: votes.length > 0 },
      {
        label: "Decision registered",
        done: Boolean(firstAgendaItem?.decisionStatus),
      },
      { label: "At least one task assigned", done: tasks.length > 0 },
    ],
    [attachments.length, firstAgendaItem, tasks.length, votes.length],
  );

  useEffect(() => {
    if (!voteForm.boardMemberEmployeeId && user?.email) {
      setVoteForm((current) => ({
        ...current,
        boardMemberEmployeeId: user.email ?? "",
      }));
    }
    if (!taskForm.responsibleEmployeeId && user?.email) {
      setTaskForm((current) => ({
        ...current,
        responsibleEmployeeId: user.email ?? "",
      }));
    }
  }, [taskForm.responsibleEmployeeId, user?.email, voteForm.boardMemberEmployeeId]);

  useEffect(() => {
    async function loadDropDowns() {
      const [
        categories,
        voteTypes,
        decisionStatuses,
        taskStatuses,
        documentTypes,
      ] = await Promise.all([
        getDropDownOptions("Category"),
        getDropDownOptions("VoteType"),
        getDropDownOptions("DecisionStatus"),
        getDropDownOptions("TaskStatus"),
        getDropDownOptions("DocumentType"),
      ]);

      setCategoryOptions(categories);
      setVoteTypeOptions(voteTypes);
      setDecisionStatusOptions(decisionStatuses);
      setTaskStatusOptions(taskStatuses);
      setDocumentTypeOptions(documentTypes);
      setVoteForm((current) => ({
        ...current,
        voteType: current.voteType || voteTypes[0]?.code || "Positive",
      }));
      setDecisionForm((current) => ({
        ...current,
        decisionStatus:
          current.decisionStatus || decisionStatuses[0]?.code || "Approved",
      }));
      setTaskForm((current) => ({
        ...current,
        status: current.status || taskStatuses[0]?.code || "ToDo",
      }));
    }

    void loadDropDowns();
  }, []);

  async function load() {
    setLoading(true);
    setError(null);
    try {
      setDetails(await boardProposalApi.search(id));
    } catch (caught) {
      setError(
        caught instanceof ApiError ? caught.message : "Could not load request",
      );
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    void load();
  }, [id]);

  async function run(label: string, action: () => Promise<void>, reload = true) {
    setWorking(label);
    setMessage(null);
    setError(null);
    try {
      await action();
      setMessage(`${label} completed`);
      if (reload) await load();
    } catch (caught) {
      setError(caught instanceof ApiError ? caught.message : `${label} failed`);
    } finally {
      setWorking(null);
    }
  }

  async function uploadSelectedAttachment(selectedFile: File | null) {
    if (!selectedFile) return;
    setFile(selectedFile);
    await run("Upload attachment", async () => {
      const form = new FormData();
      form.set("requestType", "BoardProposalRequest");
      form.set("requestId", String(id));
      form.set("section", firstAgendaItem ? "AgendaItem" : "Meeting");
      if (firstAgendaItem) {
        form.set("sectionEntityId", String(firstAgendaItem.id));
      }
      form.set("documentType", documentTypeOptions[0]?.code ?? "BoardMaterial");
      form.set("documentName", selectedFile.name);
      form.set("file", selectedFile);

      await apiFetch<number>("/attachments", {
        method: "POST",
        body: form,
      });
      setFile(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    });
  }

  async function downloadAttachment(attachmentId: number, fileName: string) {
    const blob = await apiDownload(`/attachments/${attachmentId}`);
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement("a");
    anchor.href = url;
    anchor.download = fileName;
    document.body.append(anchor);
    anchor.click();
    anchor.remove();
    URL.revokeObjectURL(url);
  }

  async function addVote(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!firstAgendaItem) return;
    await run("Add vote", async () => {
      await boardProposalApi.addVote(firstAgendaItem.id, {
        boardMemberEmployeeId: voteForm.boardMemberEmployeeId,
        voteType: voteForm.voteType,
        notes: voteForm.notes || undefined,
      });
      setVoteForm((current) => ({ ...current, notes: "" }));
    });
  }

  async function setDecision(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!firstAgendaItem) return;
    await run("Set decision", () =>
      boardProposalApi.setDecision(firstAgendaItem.id, {
        decisionStatus: decisionForm.decisionStatus,
        decisionText: decisionForm.decisionText,
        finalVote: decisionForm.finalVote || undefined,
        notes: decisionForm.notes || undefined,
      }),
    );
  }

  async function addTask(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!firstAgendaItem) return;
    await run("Add task", async () => {
      await boardProposalApi.addTask(firstAgendaItem.id, {
        title: taskForm.title,
        description: taskForm.description || undefined,
        responsibleEmployeeId: taskForm.responsibleEmployeeId,
        dueDate: new Date(taskForm.dueDate).toISOString(),
        status: taskForm.status,
      });
      setTaskForm((current) => ({
        ...current,
        title: "",
        description: "",
      }));
    });
  }

  async function reorderTask(targetTaskId: number) {
    if (!firstAgendaItem || draggedTaskId === null || draggedTaskId === targetTaskId) {
      setDraggedTaskId(null);
      return;
    }

    const currentIndex = tasks.findIndex((task) => task.id === draggedTaskId);
    const targetIndex = tasks.findIndex((task) => task.id === targetTaskId);
    if (currentIndex < 0 || targetIndex < 0) return;

    const reordered = [...tasks];
    const [moved] = reordered.splice(currentIndex, 1);
    reordered.splice(targetIndex, 0, moved);
    setDraggedTaskId(null);

    await run(
      "Reorder tasks",
      () =>
        boardProposalApi.reorderTasks(
          firstAgendaItem.id,
          reordered.map((task, index) => ({ id: task.id, order: index + 1 })),
        ),
    );
  }

  if (loading && !details) {
    return <div className="gls rounded-2xl p-6">Loading request...</div>;
  }

  return (
    <div className="space-y-6">
      <header className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="text-xs font-semibold uppercase tracking-widest text-white/45">
            Board proposal request
          </p>
          <h1 className="display mt-2 text-3xl font-bold">
            {details?.meetingCode ?? `Request ${id}`}
          </h1>
          <p className="mt-1 text-sm text-white/55">
            Business stage:{" "}
            <span className="font-semibold text-white">
              {stages[currentStageIndex]?.title}
            </span>
            <span className="ml-3 text-white/35">
              Internal status: {details?.status}
            </span>
          </p>
        </div>
        <Link href="/requests" className="btn-ghost">
          Back to requests
        </Link>
      </header>

      {error && (
        <div className="rounded-2xl border border-rose-300/30 bg-rose-400/10 p-3 text-sm text-rose-100">
          {error}
        </div>
      )}
      {message && (
        <div className="rounded-2xl border border-emerald-300/30 bg-emerald-400/10 p-3 text-sm text-emerald-100">
          {message}
        </div>
      )}

      <section className="gls rounded-2xl p-5">
        <h2 className="display text-lg font-semibold">Workflow</h2>
        <div className="mt-4 grid gap-3 md:grid-cols-5">
          {stages.map((stage, index) => (
            <div
              key={stage.title}
              className={`rounded-2xl border p-4 ${
                index < currentStageIndex
                  ? "border-emerald-300/30 bg-emerald-400/10"
                  : index === currentStageIndex
                    ? "border-fuchsia-200/45 bg-fuchsia-300/20"
                    : "border-white/10 bg-white/[0.04]"
              }`}
            >
              <span className="display text-lg font-bold">{index + 1}</span>
              <h3 className="mt-2 text-sm font-semibold">{stage.title}</h3>
              <p className="mt-1 text-xs text-white/45">{stage.description}</p>
            </div>
          ))}
        </div>
      </section>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_390px]">
        <main className="space-y-5">
          <section className="gls rounded-2xl p-5">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <h2 className="display text-lg font-semibold">Agenda item</h2>
              {firstAgendaItem?.decisionStatus && (
                <span className="chip border-emerald-300/30 bg-emerald-400/10 text-emerald-100">
                  {firstAgendaItem.decisionStatus}
                </span>
              )}
            </div>
            {firstAgendaItem ? (
              <div className="mt-4 rounded-2xl border border-white/10 bg-white/[0.05] p-4">
                <p className="font-semibold">{firstAgendaItem.title}</p>
                <p className="mt-1 text-sm text-white/55">
                  {firstAgendaItem.description}
                </p>
                <div className="mt-3 flex flex-wrap gap-2">
                  <span className="chip">
                    {getDropDownLabel(categoryOptions, firstAgendaItem.category)}
                  </span>
                  <span className="chip">
                    Presenter: {firstAgendaItem.presenterEmployeeId}
                  </span>
                  <span className="chip">
                    Board member:{" "}
                    {firstAgendaItem.responsibleBoardMemberEmployeeId}
                  </span>
                </div>
              </div>
            ) : (
              <p className="mt-3 text-sm text-white/55">No agenda item yet.</p>
            )}
          </section>

          <section className="gls rounded-2xl p-5">
            <h2 className="display text-lg font-semibold">Materials</h2>
            <div className="mt-4 flex flex-wrap items-center gap-3">
              <input
                ref={fileInputRef}
                key={attachments.length}
                type="file"
                className="hidden"
                onChange={(event) =>
                  void uploadSelectedAttachment(event.target.files?.[0] ?? null)
                }
              />
              <button
                type="button"
                className="btn-ghost"
                disabled={Boolean(working)}
                onClick={() => fileInputRef.current?.click()}
              >
                {file ? `Uploading ${file.name}` : "Upload attachment"}
              </button>
            </div>
            <div className="mt-4 space-y-2">
              {attachments.map((attachment) => (
                <div
                  key={attachment.id}
                  className="flex flex-wrap items-center justify-between gap-3 rounded-2xl border border-white/10 bg-white/[0.05] px-4 py-3 text-sm"
                >
                  <span>
                    <span className="block font-semibold">
                      {attachment.fileName}
                    </span>
                    <span className="text-xs text-white/45">
                      {attachment.documentType} -{" "}
                      {formatBytes(attachment.sizeInBytes)}
                    </span>
                  </span>
                  <span className="flex gap-2">
                    <button
                      type="button"
                      className="btn-ghost px-3 py-2 text-xs"
                      disabled={Boolean(working)}
                      onClick={() =>
                        run(
                          "Download attachment",
                          () =>
                            downloadAttachment(
                              attachment.id,
                              attachment.fileName,
                            ),
                          false,
                        )
                      }
                    >
                      Download
                    </button>
                    <button
                      type="button"
                      className="btn-ghost px-3 py-2 text-xs text-rose-100"
                      disabled={Boolean(working)}
                      onClick={() =>
                        run("Delete attachment", () =>
                          boardProposalApi.deleteAttachment(attachment.id),
                        )
                      }
                    >
                      Delete
                    </button>
                  </span>
                </div>
              ))}
              {attachments.length === 0 && (
                <p className="rounded-2xl border border-dashed border-white/15 p-4 text-sm text-white/45">
                  No materials uploaded yet.
                </p>
              )}
            </div>
          </section>

          <section className="grid gap-5 lg:grid-cols-2">
            <div className="gls rounded-2xl p-5">
              <h2 className="display text-lg font-semibold">Votes</h2>
              <form onSubmit={addVote} className="mt-4 space-y-3">
                <input
                  className="field"
                  placeholder="Board member employee id"
                  value={voteForm.boardMemberEmployeeId}
                  onChange={(event) =>
                    setVoteForm((current) => ({
                      ...current,
                      boardMemberEmployeeId: event.target.value,
                    }))
                  }
                  required
                />
                <select
                  className="field"
                  value={voteForm.voteType}
                  onChange={(event) =>
                    setVoteForm((current) => ({
                      ...current,
                      voteType: event.target.value,
                    }))
                  }
                >
                  {voteTypeOptions.map((voteType) => (
                    <option key={voteType.code} value={voteType.code}>
                      {voteType.label}
                    </option>
                  ))}
                </select>
                <textarea
                  className="field min-h-20 resize-y"
                  placeholder="Vote notes"
                  value={voteForm.notes}
                  onChange={(event) =>
                    setVoteForm((current) => ({
                      ...current,
                      notes: event.target.value,
                    }))
                  }
                />
                <button
                  type="submit"
                  className="btn-ghost w-full"
                  disabled={!firstAgendaItem || Boolean(working)}
                >
                  Add vote
                </button>
              </form>
              <div className="mt-4 space-y-2">
                {votes.map((vote) => (
                  <div
                    key={vote.id}
                    className="rounded-2xl border border-white/10 bg-white/[0.05] p-3 text-sm"
                  >
                    <p className="font-semibold">
                      {getDropDownLabel(voteTypeOptions, vote.voteType)}
                    </p>
                    <p className="text-xs text-white/45">
                      {vote.boardMemberEmployeeId}
                    </p>
                    {vote.notes && (
                      <p className="mt-1 text-white/55">{vote.notes}</p>
                    )}
                  </div>
                ))}
                {votes.length === 0 && (
                  <p className="text-sm text-white/45">No votes captured yet.</p>
                )}
              </div>
            </div>

            <div className="gls rounded-2xl p-5">
              <h2 className="display text-lg font-semibold">Decision</h2>
              <form onSubmit={setDecision} className="mt-4 space-y-3">
                <select
                  className="field"
                  value={decisionForm.decisionStatus}
                  onChange={(event) =>
                    setDecisionForm((current) => ({
                      ...current,
                      decisionStatus: event.target.value,
                    }))
                  }
                >
                  {decisionStatusOptions.map((status) => (
                    <option key={status.code} value={status.code}>
                      {status.label}
                    </option>
                  ))}
                </select>
                <textarea
                  className="field min-h-20 resize-y"
                  placeholder="Decision text"
                  value={decisionForm.decisionText}
                  onChange={(event) =>
                    setDecisionForm((current) => ({
                      ...current,
                      decisionText: event.target.value,
                    }))
                  }
                  required
                />
                <input
                  className="field"
                  placeholder="Final vote summary"
                  value={decisionForm.finalVote}
                  onChange={(event) =>
                    setDecisionForm((current) => ({
                      ...current,
                      finalVote: event.target.value,
                    }))
                  }
                />
                <button
                  type="submit"
                  className="btn-ghost w-full"
                  disabled={!firstAgendaItem || Boolean(working)}
                >
                  Save decision
                </button>
              </form>
              {firstAgendaItem?.decisionStatus ? (
                <div className="mt-4 rounded-2xl border border-emerald-300/20 bg-emerald-400/10 p-4 text-sm">
                  <p className="font-semibold">
                    {getDropDownLabel(
                      decisionStatusOptions,
                      firstAgendaItem.decisionStatus,
                    )}
                  </p>
                  <p className="mt-1 text-white/65">
                    {firstAgendaItem.decisionText}
                  </p>
                  {firstAgendaItem.finalVote && (
                    <p className="mt-2 text-xs text-white/45">
                      Final vote: {firstAgendaItem.finalVote}
                    </p>
                  )}
                </div>
              ) : (
                <p className="mt-4 text-sm text-white/45">
                  No decision registered yet.
                </p>
              )}
            </div>
          </section>

          <section className="gls rounded-2xl p-5">
            <h2 className="display text-lg font-semibold">Tasks</h2>
            <form onSubmit={addTask} className="mt-4 grid gap-3 md:grid-cols-2">
              <input
                className="field"
                placeholder="Task title"
                value={taskForm.title}
                onChange={(event) =>
                  setTaskForm((current) => ({
                    ...current,
                    title: event.target.value,
                  }))
                }
                required
              />
              <input
                className="field"
                placeholder="Responsible employee id"
                value={taskForm.responsibleEmployeeId}
                onChange={(event) =>
                  setTaskForm((current) => ({
                    ...current,
                    responsibleEmployeeId: event.target.value,
                  }))
                }
                required
              />
              <input
                type="datetime-local"
                className="field"
                value={taskForm.dueDate}
                onChange={(event) =>
                  setTaskForm((current) => ({
                    ...current,
                    dueDate: event.target.value,
                  }))
                }
                required
              />
              <select
                className="field"
                value={taskForm.status}
                onChange={(event) =>
                  setTaskForm((current) => ({
                    ...current,
                    status: event.target.value,
                  }))
                }
              >
                {taskStatusOptions.map((status) => (
                  <option key={status.code} value={status.code}>
                    {status.label}
                  </option>
                ))}
              </select>
              <textarea
                className="field min-h-20 resize-y md:col-span-2"
                placeholder="Task description"
                value={taskForm.description}
                onChange={(event) =>
                  setTaskForm((current) => ({
                    ...current,
                    description: event.target.value,
                  }))
                }
              />
              <button
                type="submit"
                className="btn-ghost md:col-span-2"
                disabled={!firstAgendaItem || Boolean(working)}
              >
                Add task
              </button>
            </form>
            <div className="mt-4 space-y-2">
              {tasks.map((task) => (
                <div
                  key={task.id}
                  draggable
                  onDragStart={() => setDraggedTaskId(task.id)}
                  onDragOver={(event: DragEvent<HTMLDivElement>) =>
                    event.preventDefault()
                  }
                  onDrop={() => void reorderTask(task.id)}
                  className="grid cursor-move gap-2 rounded-2xl border border-white/10 bg-white/[0.05] p-4 text-sm md:grid-cols-[auto_1fr_auto]"
                >
                  <span className="text-white/35">::</span>
                  <span>
                    <span className="block font-semibold">{task.title}</span>
                    <span className="text-xs text-white/45">
                      {task.responsibleEmployeeId} - Due{" "}
                      {new Date(task.dueDate).toLocaleDateString()}
                    </span>
                    {task.description && (
                      <span className="mt-1 block text-white/55">
                        {task.description}
                      </span>
                    )}
                  </span>
                  <span className="chip">
                    {getDropDownLabel(taskStatusOptions, task.status)}
                  </span>
                </div>
              ))}
              {tasks.length === 0 && (
                <p className="rounded-2xl border border-dashed border-white/15 p-4 text-sm text-white/45">
                  No tasks assigned yet.
                </p>
              )}
            </div>
          </section>
        </main>

        <aside className="space-y-5">
          <section className="gls rounded-2xl p-5">
            <h2 className="display text-lg font-semibold">Next action</h2>
            <p className="mt-2 text-sm text-white/50">
              {nextAction?.note ?? "No available workflow action."}
            </p>
            <div className="mt-4 flex flex-col gap-3">
              <button
                type="button"
                className="btn-primary"
                disabled={!nextAction || Boolean(working)}
                onClick={() =>
                  nextAction &&
                  run(nextAction.label, () =>
                    boardProposalApi.nextStep(id, nextAction.action),
                  )
                }
              >
                {working ?? nextAction?.label ?? "No action"}
              </button>
              {canCancel && (
                <button
                  type="button"
                  className="btn-ghost"
                  disabled={Boolean(working)}
                  onClick={() =>
                    run("Cancel", () => boardProposalApi.nextStep(id, "Cancel"))
                  }
                >
                  Cancel request
                </button>
              )}
            </div>
          </section>

          <section className="gls rounded-2xl p-5">
            <h2 className="display text-lg font-semibold">Readiness</h2>
            <div className="mt-4 space-y-2">
              {readiness.map((item) => (
                <div key={item.label} className="flex items-center gap-3 text-sm">
                  <span
                    className={`grid h-6 w-6 place-items-center rounded-lg border text-xs ${
                      item.done
                        ? "border-emerald-300/40 bg-emerald-400/10 text-emerald-100"
                        : "border-amber-300/30 bg-amber-400/10 text-amber-100"
                    }`}
                  >
                    {item.done ? "OK" : "!"}
                  </span>
                  <span className="text-white/65">{item.label}</span>
                </div>
              ))}
            </div>
          </section>
        </aside>
      </div>
    </div>
  );
}

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
