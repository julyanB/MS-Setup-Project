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
  AttachmentDetails,
  BoardProposalAgendaItemDetails,
  BoardProposalDecisionStatus,
  BoardProposalRequestDetails,
  BoardProposalStatus,
  BoardProposalTaskStatus,
  BoardProposalVoteDetails,
  BoardProposalVoteType,
  EmployeeRequestAction,
  boardProposalApi,
} from "@/lib/boardProposals";
import {
  DropDownOption,
  getDropDownLabel,
  getDropDownOptions,
} from "@/lib/dropDowns";
import {
  employeeApi,
  employeeLabel,
  type EmployeeLookupItem,
} from "@/lib/employees";

const BOARD_PROPOSAL_BOARD_MEMBER_ROLE = "BoardProposalBoardMember";
const BOARD_PROPOSAL_TASK_OWNER_ROLE = "BoardProposalTaskOwner";

type Stage = {
  title: string;
  description: string;
  statuses: BoardProposalStatus[];
};

type PrimaryAction = {
  action: EmployeeRequestAction;
  label: string;
  note: string;
};

const stages: Stage[] = [
  {
    title: "Draft",
    description: "Meeting is created and agenda preparation starts.",
    statuses: ["Draft", "AgendaPreparation"],
  },
  {
    title: "Approval",
    description: "Secretary and chairperson validate the agenda.",
    statuses: ["SecretaryReview", "ChairpersonReview", "ReadyForSending"],
  },
  {
    title: "Meeting",
    description: "Approved package is sent and marked as held.",
    statuses: ["Sent", "Held"],
  },
  {
    title: "Execution",
    description: "Votes, decisions, and follow-up tasks are registered.",
    statuses: ["DecisionsAndTasks", "DeadlineMonitoring"],
  },
  {
    title: "Archive",
    description: "Request is closed or cancelled.",
    statuses: ["Closed", "Cancelled"],
  },
];

const primaryActionByStatus: Partial<Record<BoardProposalStatus, PrimaryAction>> = {
  Draft: {
    action: "Submit",
    label: "Start agenda preparation",
    note: "Moves the request from draft into agenda preparation.",
  },
  AgendaPreparation: {
    action: "Submit",
    label: "Submit for secretary review",
    note: "Requires every agenda item to have owner, presenter, category, order, and material.",
  },
  SecretaryReview: {
    action: "Approve",
    label: "Secretary approve",
    note: "Secretary confirms the agenda package is complete.",
  },
  ChairpersonReview: {
    action: "Approve",
    label: "Chairperson approve",
    note: "Chairperson confirms agenda order and priority.",
  },
  ReadyForSending: {
    action: "Send",
    label: "Send agenda",
    note: "Marks the approved agenda package as sent.",
  },
  Sent: {
    action: "MarkHeld",
    label: "Mark meeting held",
    note: "Can be done after the meeting date has passed.",
  },
  Held: {
    action: "StartDecisionRegistration",
    label: "Start decision registration",
    note: "Opens the decision and task registration step.",
  },
  DecisionsAndTasks: {
    action: "StartMonitoring",
    label: "Start monitoring",
    note: "Requires a final decision for every item and tasks for approved decisions.",
  },
  DeadlineMonitoring: {
    action: "Close",
    label: "Close request",
    note: "Requires all tasks to be completed, cancelled, or not applicable.",
  },
  Closed: {
    action: "Reopen",
    label: "Reopen deadline monitoring",
    note: "Returns the request to monitoring.",
  },
};

const closedTaskStatuses: BoardProposalTaskStatus[] = [
  "Completed",
  "Cancelled",
  "NotApplicable",
];

function getSecondaryActions(
  status: BoardProposalStatus | undefined,
): { action: EmployeeRequestAction; label: string; tone?: "danger" }[] {
  if (status === "SecretaryReview" || status === "ChairpersonReview") {
    return [
      { action: "Return", label: "Return for correction" },
      { action: "Reject", label: "Reject request", tone: "danger" },
    ];
  }

  return [];
}

function canManageAgenda(status: BoardProposalStatus | undefined) {
  return status === "Draft" || status === "AgendaPreparation";
}

function canReorderAgenda(status: BoardProposalStatus | undefined) {
  return (
    status === "Draft" ||
    status === "AgendaPreparation" ||
    status === "SecretaryReview" ||
    status === "ChairpersonReview"
  );
}

function canManageMaterials(status: BoardProposalStatus | undefined) {
  return (
    status === "Draft" ||
    status === "AgendaPreparation" ||
    status === "SecretaryReview"
  );
}

function canCaptureVotes(status: BoardProposalStatus | undefined) {
  return status === "Held" || status === "DecisionsAndTasks";
}

function canSetDecision(status: BoardProposalStatus | undefined) {
  return status === "DecisionsAndTasks";
}

function canEditTasks(status: BoardProposalStatus | undefined) {
  return status === "DecisionsAndTasks";
}

function canUpdateTaskStatus(status: BoardProposalStatus | undefined) {
  return status === "DecisionsAndTasks" || status === "DeadlineMonitoring";
}

function calculateFinalVote(votes: BoardProposalVoteDetails[] | undefined) {
  let positive = 0;
  let negative = 0;

  for (const vote of votes ?? []) {
    if (
      vote.voteType === "Positive" ||
      vote.voteType === "PositiveWithCondition" ||
      vote.voteType === "PositiveWithRecommendation"
    ) {
      positive += 1;
    } else if (
      vote.voteType === "Negative" ||
      vote.voteType === "NegativeWithComments"
    ) {
      negative += 1;
    }
  }

  if (positive === 0 && negative === 0) {
    return "NoVotes";
  }

  if (positive === negative) {
    return "Tie";
  }

  return positive > negative ? "Positive" : "Negative";
}

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
  const [selectedAgendaItemId, setSelectedAgendaItemId] = useState<number | null>(
    null,
  );
  const [loading, setLoading] = useState(true);
  const [working, setWorking] = useState<string | null>(null);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [draggedAgendaItemId, setDraggedAgendaItemId] = useState<number | null>(
    null,
  );
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
  const [employees, setEmployees] = useState<EmployeeLookupItem[]>([]);
  const [boardMemberEmployees, setBoardMemberEmployees] = useState<
    EmployeeLookupItem[]
  >([]);
  const [taskOwnerEmployees, setTaskOwnerEmployees] = useState<
    EmployeeLookupItem[]
  >([]);
  const [agendaForm, setAgendaForm] = useState({
    title: "",
    initiatorEmployeeId: "",
    responsibleBoardMemberEmployeeId: "",
    presenterEmployeeId: "",
    category: "Business",
    description: "",
  });
  const [voteForm, setVoteForm] = useState<{
    boardMemberEmployeeId: string;
    voteType: BoardProposalVoteType;
    notes: string;
  }>({
    boardMemberEmployeeId: "",
    voteType: "Positive",
    notes: "",
  });
  const [decisionForm, setDecisionForm] = useState<{
    decisionStatus: BoardProposalDecisionStatus;
    decisionText: string;
    notes: string;
  }>({
    decisionStatus: "Approved",
    decisionText: "",
    notes: "",
  });
  const [taskForm, setTaskForm] = useState<{
    title: string;
    description: string;
    responsibleEmployeeId: string;
    dueDate: string;
    status: BoardProposalTaskStatus;
  }>({
    title: "",
    description: "",
    responsibleEmployeeId: "",
    dueDate: new Date(Date.now() + 7 * 86400000).toISOString().slice(0, 16),
    status: "ToDo",
  });

  const agendaItems = useMemo(
    () => [...(details?.agendaItems ?? [])].sort((a, b) => a.order - b.order),
    [details?.agendaItems],
  );
  const attachments = details?.attachments ?? [];
  const selectedAgendaItem =
    agendaItems.find((item) => item.id === selectedAgendaItemId) ??
    agendaItems[0];
  const selectedAttachments = selectedAgendaItem
    ? getAgendaAttachments(attachments, selectedAgendaItem.id)
    : [];
  const selectedTasks = useMemo(
    () =>
      [...(selectedAgendaItem?.tasks ?? [])].sort(
        (a, b) =>
          (a.order ?? Number.MAX_SAFE_INTEGER) -
            (b.order ?? Number.MAX_SAFE_INTEGER) ||
          new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime(),
      ),
    [selectedAgendaItem?.tasks],
  );
  const allTasks = agendaItems.flatMap((item) => item.tasks);
  const primaryAction = details
    ? primaryActionByStatus[details.status]
    : undefined;
  const secondaryActions = getSecondaryActions(details?.status);
  const isApprovalStatus =
    details?.status === "SecretaryReview" || details?.status === "ChairpersonReview";
  const isActiveApprover = Boolean(
    details?.activeApprovalTargets?.some((target) =>
      target.targetType === "User"
        ? target.targetValue === user?.id
        : user?.roles.includes(target.targetValue),
    ),
  );
  const approvalBlockReason =
    isApprovalStatus && !isActiveApprover
      ? "This request is waiting for a different approval role or user."
      : null;
  const currentStageIndex = details
    ? Math.max(
        0,
        stages.findIndex((stage) => stage.statuses.includes(details.status)),
      )
    : 0;
  const isTerminal = details?.status === "Closed" || details?.status === "Cancelled";
  const mayCancel = Boolean(details && !isTerminal);
  const primaryBlockReason = useMemo(
    () => getPrimaryBlockReason(details, attachments),
    [details, attachments],
  );
  const readiness = useMemo(
    () => getReadiness(details, attachments),
    [details, attachments],
  );

  useEffect(() => {
    async function loadEmployees() {
      const [all, boardMembers, taskOwners] = await Promise.all([
        employeeApi.lookup({ limit: 250 }),
        employeeApi.lookup({
          role: BOARD_PROPOSAL_BOARD_MEMBER_ROLE,
          limit: 250,
        }),
        employeeApi.lookup({
          role: BOARD_PROPOSAL_TASK_OWNER_ROLE,
          limit: 250,
        }),
      ]);

      setEmployees(all);
      setBoardMemberEmployees(boardMembers);
      setTaskOwnerEmployees(taskOwners);

      setAgendaForm((current) => ({
        ...current,
        initiatorEmployeeId:
          current.initiatorEmployeeId || user?.id || all[0]?.id || "",
        presenterEmployeeId:
          current.presenterEmployeeId || user?.id || all[0]?.id || "",
        responsibleBoardMemberEmployeeId:
          current.responsibleBoardMemberEmployeeId || boardMembers[0]?.id || "",
      }));
      setVoteForm((current) => ({
        ...current,
        boardMemberEmployeeId:
          current.boardMemberEmployeeId || boardMembers[0]?.id || "",
      }));
      setTaskForm((current) => ({
        ...current,
        responsibleEmployeeId:
          current.responsibleEmployeeId || taskOwners[0]?.id || all[0]?.id || "",
      }));
    }

    void loadEmployees();
  }, [user?.id]);

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
      setAgendaForm((current) => ({
        ...current,
        category: current.category || categories[0]?.code || "Business",
      }));
      setVoteForm((current) => ({
        ...current,
        voteType:
          current.voteType ||
          ((voteTypes[0]?.code ?? "Positive") as BoardProposalVoteType),
      }));
      setDecisionForm((current) => ({
        ...current,
        decisionStatus:
          current.decisionStatus ||
          ((decisionStatuses[0]?.code ?? "Approved") as BoardProposalDecisionStatus),
      }));
      setTaskForm((current) => ({
        ...current,
        status:
          current.status ||
          ((taskStatuses[0]?.code ?? "ToDo") as BoardProposalTaskStatus),
      }));
    }

    void loadDropDowns();
  }, []);

  useEffect(() => {
    if (agendaItems.length === 0) {
      setSelectedAgendaItemId(null);
      return;
    }

    if (!selectedAgendaItemId || !agendaItems.some((x) => x.id === selectedAgendaItemId)) {
      setSelectedAgendaItemId(agendaItems[0].id);
    }
  }, [agendaItems, selectedAgendaItemId]);

  useEffect(() => {
    if (!selectedAgendaItem) return;
    setDecisionForm({
      decisionStatus: selectedAgendaItem.decisionStatus ?? "Approved",
      decisionText: selectedAgendaItem.decisionText ?? "",
      notes: selectedAgendaItem.notes ?? "",
    });
  }, [selectedAgendaItem?.id]);

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

  async function addAgendaItem(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    await run("Add agenda item", async () => {
      const agendaItemId = await boardProposalApi.addAgendaItem(id, {
        title: agendaForm.title,
        initiatorEmployeeId: agendaForm.initiatorEmployeeId,
        responsibleBoardMemberEmployeeId:
          agendaForm.responsibleBoardMemberEmployeeId,
        presenterEmployeeId: agendaForm.presenterEmployeeId,
        category: agendaForm.category,
        description: agendaForm.description || undefined,
      });
      setSelectedAgendaItemId(agendaItemId);
      setAgendaForm((current) => ({
        ...current,
        title: "",
        description: "",
        responsibleBoardMemberEmployeeId: "",
      }));
    });
  }

  async function uploadSelectedAttachment(selectedFile: File | null) {
    if (!selectedFile || !selectedAgendaItem) return;

    await run("Upload attachment", async () => {
      const form = new FormData();
      form.set("requestType", "BoardProposalRequest");
      form.set("requestId", String(id));
      form.set("section", "AgendaItem");
      form.set("sectionEntityId", String(selectedAgendaItem.id));
      form.set("documentType", documentTypeOptions[0]?.code ?? "BoardMaterial");
      form.set("documentName", selectedFile.name);
      form.set("file", selectedFile);

      await apiFetch<number>("/attachments", {
        method: "POST",
        body: form,
      });

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
    if (!selectedAgendaItem) return;
    await run("Add vote", async () => {
      await boardProposalApi.addVote(selectedAgendaItem.id, {
        boardMemberEmployeeId: voteForm.boardMemberEmployeeId,
        voteType: voteForm.voteType,
        notes: voteForm.notes || undefined,
      });
      setVoteForm((current) => ({ ...current, notes: "" }));
    });
  }

  async function setDecision(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selectedAgendaItem) return;
    await run("Save decision", () =>
      boardProposalApi.setDecision(selectedAgendaItem.id, {
        decisionStatus: decisionForm.decisionStatus,
        decisionText: decisionForm.decisionText,
        notes: decisionForm.notes || undefined,
      }),
    );
  }

  async function addTask(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!selectedAgendaItem) return;
    await run("Add task", async () => {
      await boardProposalApi.addTask(selectedAgendaItem.id, {
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

  async function reorderAgendaItem(targetAgendaItemId: number) {
    if (
      draggedAgendaItemId === null ||
      draggedAgendaItemId === targetAgendaItemId
    ) {
      setDraggedAgendaItemId(null);
      return;
    }

    const currentIndex = agendaItems.findIndex(
      (item) => item.id === draggedAgendaItemId,
    );
    const targetIndex = agendaItems.findIndex(
      (item) => item.id === targetAgendaItemId,
    );
    if (currentIndex < 0 || targetIndex < 0) return;

    const reordered = [...agendaItems];
    const [moved] = reordered.splice(currentIndex, 1);
    reordered.splice(targetIndex, 0, moved);
    setDraggedAgendaItemId(null);

    await run("Reorder agenda items", () =>
      boardProposalApi.reorderAgendaItems(
        id,
        reordered.map((item, index) => ({ id: item.id, order: index + 1 })),
      ),
    );
  }

  async function reorderTask(targetTaskId: number) {
    if (!selectedAgendaItem || draggedTaskId === null || draggedTaskId === targetTaskId) {
      setDraggedTaskId(null);
      return;
    }

    const currentIndex = selectedTasks.findIndex((task) => task.id === draggedTaskId);
    const targetIndex = selectedTasks.findIndex((task) => task.id === targetTaskId);
    if (currentIndex < 0 || targetIndex < 0) return;

    const reordered = [...selectedTasks];
    const [moved] = reordered.splice(currentIndex, 1);
    reordered.splice(targetIndex, 0, moved);
    setDraggedTaskId(null);

    await run("Reorder tasks", () =>
      boardProposalApi.reorderTasks(
        selectedAgendaItem.id,
        reordered.map((task, index) => ({ id: task.id, order: index + 1 })),
      ),
    );
  }

  async function updateTaskStatus(
    taskId: number,
    status: BoardProposalTaskStatus,
  ) {
    await run("Update task status", () =>
      boardProposalApi.updateTaskStatus(taskId, { status }),
    );
  }

  if (loading && !details) {
    return <div className="gls rounded-2xl p-6">Loading request...</div>;
  }

  if (!details) {
    return (
      <div className="space-y-5">
        <header className="flex flex-wrap items-end justify-between gap-4">
          <div>
            <p className="text-xs font-semibold uppercase tracking-widest text-white/45">
              Board proposal request
            </p>
            <h1 className="display mt-2 text-3xl font-bold">Request {id}</h1>
          </div>
          <Link href="/requests" className="btn-ghost">
            Back to requests
          </Link>
        </header>
        <div className="whitespace-pre-line rounded-2xl border border-rose-300/30 bg-rose-400/10 p-5 text-sm text-rose-100">
          {error ?? "Could not load request."}
        </div>
      </div>
    );
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
            Status:{" "}
            <span className="font-semibold text-white">{details?.status}</span>
            <span className="ml-3 text-white/35">
              Meeting:{" "}
              {details?.meetingDate
                ? new Date(details.meetingDate).toLocaleString()
                : "-"}
            </span>
          </p>
        </div>
        <Link href="/requests" className="btn-ghost">
          Back to requests
        </Link>
      </header>

      {error && (
        <div className="whitespace-pre-line rounded-2xl border border-rose-300/30 bg-rose-400/10 p-3 text-sm text-rose-100">
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
        <div className="mt-5 overflow-x-auto pb-2">
          <div className="relative grid min-w-[760px] grid-cols-5 gap-0">
            <div className="absolute left-[10%] right-[10%] top-7 border-t-2 border-dashed border-white/25" />
            {stages.map((stage, index) => (
              <div
                key={stage.title}
                className="relative z-10 flex flex-col items-center text-center"
              >
                <div
                  className={`grid h-14 w-14 place-items-center rounded-full border text-lg font-bold shadow-lg ${
                    index < currentStageIndex
                      ? "border-emerald-200/60 bg-emerald-400/25 text-emerald-50 shadow-emerald-950/20"
                      : index === currentStageIndex
                        ? "border-fuchsia-100/80 bg-fuchsia-400/35 text-white shadow-fuchsia-950/30"
                        : "border-white/20 bg-white/[0.06] text-white/65 shadow-black/10"
                  }`}
                >
                  {index + 1}
                </div>
                <h3 className="mt-3 text-sm font-semibold">{stage.title}</h3>
                <p className="mt-1 text-xs leading-snug text-white/45">
                  {stage.statuses.join(" / ")}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_390px]">
        <main className="space-y-5">
          <section className="gls rounded-2xl p-5">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <h2 className="display text-lg font-semibold">Agenda items</h2>
              <span className="text-sm text-white/45">
                {agendaItems.length} item{agendaItems.length === 1 ? "" : "s"}
                {canReorderAgenda(details?.status) && agendaItems.length > 1
                  ? " - drag to reorder"
                  : ""}
              </span>
            </div>

            <div className="mt-4 grid gap-3 lg:grid-cols-2">
              {agendaItems.map((item) => {
                const itemAttachments = getAgendaAttachments(attachments, item.id);
                const isSelected = selectedAgendaItem?.id === item.id;

                return (
                  <div
                    key={item.id}
                    onClick={() => setSelectedAgendaItemId(item.id)}
                    onKeyDown={(event) => {
                      if (event.key === "Enter" || event.key === " ") {
                        event.preventDefault();
                        setSelectedAgendaItemId(item.id);
                      }
                    }}
                    role="button"
                    tabIndex={0}
                    draggable={canReorderAgenda(details?.status)}
                    onDragStart={() => setDraggedAgendaItemId(item.id)}
                    onDragEnd={() => setDraggedAgendaItemId(null)}
                    onDragOver={(event: DragEvent<HTMLDivElement>) =>
                      canReorderAgenda(details?.status) && event.preventDefault()
                    }
                    onDrop={() =>
                      canReorderAgenda(details?.status) &&
                      void reorderAgendaItem(item.id)
                    }
                    className={`rounded-2xl border p-4 text-left transition ${
                      isSelected
                        ? "border-fuchsia-200/60 bg-fuchsia-300/20"
                        : "border-white/10 bg-white/[0.04] hover:bg-white/[0.07]"
                    } ${canReorderAgenda(details?.status) ? "cursor-move" : "cursor-pointer"}`}
                  >
                    <div className="flex items-start justify-between gap-3">
                      <div>
                        <p className="text-xs font-semibold uppercase tracking-widest text-white/40">
                          {canReorderAgenda(details?.status) ? ":: " : ""}
                          #{item.order} {getDropDownLabel(categoryOptions, item.category)}
                        </p>
                        <h3 className="mt-2 font-semibold">{item.title}</h3>
                      </div>
                      <span className="chip">
                        {itemAttachments.length > 0 ? "Material" : "Missing material"}
                      </span>
                    </div>
                    <p className="mt-2 line-clamp-2 text-sm text-white/50">
                      {item.description || "No description."}
                    </p>
                    <div className="mt-3 grid gap-1 text-xs text-white/45">
                      <span>
                        Presenter: {getEmployeeDisplayName(item.presenterEmployeeId, employees)}
                      </span>
                      <span>
                        Owner:{" "}
                        {getEmployeeDisplayName(
                          item.responsibleBoardMemberEmployeeId,
                          boardMemberEmployees,
                          employees,
                        )}
                      </span>
                      <span>
                        Decision:{" "}
                        {getDropDownLabel(decisionStatusOptions, item.decisionStatus) ||
                          "Not registered"}
                      </span>
                      <span>
                        Votes: {item.votes.length} / Tasks: {item.tasks.length}
                      </span>
                    </div>
                  </div>
                );
              })}

              {agendaItems.length === 0 && (
                <p className="rounded-2xl border border-dashed border-white/15 p-4 text-sm text-white/45 lg:col-span-2">
                  No agenda items yet. Add at least one agenda item before
                  submitting for secretary review.
                </p>
              )}
            </div>

            {canManageAgenda(details?.status) ? (
              <form onSubmit={addAgendaItem} className="mt-5 grid gap-3 md:grid-cols-2">
                <div className="md:col-span-2">
                  <label className="label">Agenda item title</label>
                  <input
                    className="field"
                    value={agendaForm.title}
                    onChange={(event) =>
                      setAgendaForm((current) => ({
                        ...current,
                        title: event.target.value,
                      }))
                    }
                    maxLength={1000}
                    required
                  />
                </div>
                <div>
                  <label className="label">Presenter employee</label>
                  <EmployeeSelect
                    employees={employees}
                    value={agendaForm.presenterEmployeeId}
                    placeholder="Select presenter"
                    onChange={(value) =>
                      setAgendaForm((current) => ({
                        ...current,
                        presenterEmployeeId: value,
                      }))
                    }
                    required
                  />
                </div>
                <div>
                  <label className="label">Responsible board member</label>
                  <EmployeeSelect
                    employees={boardMemberEmployees}
                    value={agendaForm.responsibleBoardMemberEmployeeId}
                    placeholder="Select board member"
                    onChange={(value) =>
                      setAgendaForm((current) => ({
                        ...current,
                        responsibleBoardMemberEmployeeId: value,
                      }))
                    }
                    required
                  />
                  {boardMemberEmployees.length === 0 && (
                    <p className="mt-2 text-xs text-amber-100/80">
                      No users have the BoardProposalBoardMember role.
                    </p>
                  )}
                </div>
                <div>
                  <label className="label">Initiator employee</label>
                  <EmployeeSelect
                    employees={employees}
                    value={agendaForm.initiatorEmployeeId}
                    placeholder="Select initiator"
                    onChange={(value) =>
                      setAgendaForm((current) => ({
                        ...current,
                        initiatorEmployeeId: value,
                      }))
                    }
                    required
                  />
                </div>
                <div>
                  <label className="label">Category</label>
                  <select
                    className="field"
                    value={agendaForm.category}
                    onChange={(event) =>
                      setAgendaForm((current) => ({
                        ...current,
                        category: event.target.value,
                      }))
                    }
                  >
                    {categoryOptions.map((option) => (
                      <option key={option.code} value={option.code}>
                        {option.label}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="md:col-span-2">
                  <label className="label">Description</label>
                  <textarea
                    className="field min-h-24 resize-y"
                    value={agendaForm.description}
                    onChange={(event) =>
                      setAgendaForm((current) => ({
                        ...current,
                        description: event.target.value,
                      }))
                    }
                    maxLength={1500}
                  />
                </div>
                <button
                  type="submit"
                  className="btn-ghost md:col-span-2"
                  disabled={Boolean(working)}
                >
                  Add agenda item
                </button>
              </form>
            ) : (
              <p className="mt-4 rounded-2xl border border-white/10 bg-white/[0.04] p-4 text-sm text-white/45">
                Agenda items are locked after submission for secretary review.
              </p>
            )}
          </section>

          <section className="gls rounded-2xl p-5">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <div>
                <h2 className="display text-lg font-semibold">Selected item</h2>
                <p className="mt-1 text-sm text-white/45">
                  {selectedAgendaItem
                    ? selectedAgendaItem.title
                    : "Select or create an agenda item."}
                </p>
              </div>
              {selectedAgendaItem && <span className="chip">#{selectedAgendaItem.order}</span>}
            </div>
          </section>

          <section className="gls rounded-2xl p-5">
            <div className="flex flex-wrap items-center justify-between gap-3">
              <h2 className="display text-lg font-semibold">Materials</h2>
              {canManageMaterials(details?.status) && (
                <>
                  <input
                    ref={fileInputRef}
                    type="file"
                    className="hidden"
                    disabled={!selectedAgendaItem || Boolean(working)}
                    onChange={(event) =>
                      void uploadSelectedAttachment(event.target.files?.[0] ?? null)
                    }
                  />
                  <button
                    type="button"
                    className="btn-ghost"
                    disabled={!selectedAgendaItem || Boolean(working)}
                    onClick={() => fileInputRef.current?.click()}
                  >
                    Upload attachment
                  </button>
                </>
              )}
            </div>
            {!canManageMaterials(details?.status) && (
              <p className="mt-3 text-sm text-white/45">
                Materials are locked after review starts.
              </p>
            )}
            <div className="mt-4 space-y-2">
              {selectedAttachments.map((attachment) => (
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
                    {canManageMaterials(details?.status) && (
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
                    )}
                  </span>
                </div>
              ))}
              {selectedAttachments.length === 0 && (
                <p className="rounded-2xl border border-dashed border-white/15 p-4 text-sm text-white/45">
                  No materials uploaded for the selected agenda item.
                </p>
              )}
            </div>
          </section>

          <section className="grid gap-5 lg:grid-cols-2">
            <div className="gls rounded-2xl p-5">
              <h2 className="display text-lg font-semibold">Votes</h2>
              {canCaptureVotes(details?.status) ? (
                <form onSubmit={addVote} className="mt-4 space-y-3">
                  <EmployeeSelect
                    employees={boardMemberEmployees}
                    value={voteForm.boardMemberEmployeeId}
                    placeholder="Select board member"
                    onChange={(value) =>
                      setVoteForm((current) => ({
                        ...current,
                        boardMemberEmployeeId: value,
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
                        voteType: event.target.value as BoardProposalVoteType,
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
                    disabled={!selectedAgendaItem || Boolean(working)}
                  >
                    Add vote
                  </button>
                </form>
              ) : (
                <p className="mt-4 rounded-2xl border border-white/10 bg-white/[0.04] p-4 text-sm text-white/45">
                  Voting opens after the meeting is marked as held.
                </p>
              )}
              <div className="mt-4 space-y-2">
                {(selectedAgendaItem?.votes ?? []).map((vote) => (
                  <div
                    key={vote.id}
                    className="rounded-2xl border border-white/10 bg-white/[0.05] p-3 text-sm"
                  >
                    <p className="font-semibold">
                      {getDropDownLabel(voteTypeOptions, vote.voteType)}
                    </p>
                    <p className="text-xs text-white/45">
                      {getEmployeeDisplayName(
                        vote.boardMemberEmployeeId,
                        boardMemberEmployees,
                        employees,
                      )}
                    </p>
                    {vote.notes && (
                      <p className="mt-1 text-white/55">{vote.notes}</p>
                    )}
                  </div>
                ))}
                {(selectedAgendaItem?.votes.length ?? 0) === 0 && (
                  <p className="text-sm text-white/45">No votes captured yet.</p>
                )}
              </div>
            </div>

            <div className="gls rounded-2xl p-5">
              <h2 className="display text-lg font-semibold">Decision</h2>
              {canSetDecision(details?.status) ? (
                <form onSubmit={setDecision} className="mt-4 space-y-3">
                  <select
                    className="field"
                    value={decisionForm.decisionStatus}
                    onChange={(event) =>
                      setDecisionForm((current) => ({
                        ...current,
                        decisionStatus: event.target
                          .value as BoardProposalDecisionStatus,
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
                  <div className="rounded-2xl border border-white/10 bg-white/[0.04] p-3 text-sm text-white/65">
                    Calculated final vote:{" "}
                    <span className="font-semibold text-white">
                      {calculateFinalVote(selectedAgendaItem?.votes)}
                    </span>
                  </div>
                  <textarea
                    className="field min-h-20 resize-y"
                    placeholder="Decision notes"
                    value={decisionForm.notes}
                    onChange={(event) =>
                      setDecisionForm((current) => ({
                        ...current,
                        notes: event.target.value,
                      }))
                    }
                  />
                  <button
                    type="submit"
                    className="btn-ghost w-full"
                    disabled={!selectedAgendaItem || Boolean(working)}
                  >
                    Save decision
                  </button>
                </form>
              ) : (
                <p className="mt-4 rounded-2xl border border-white/10 bg-white/[0.04] p-4 text-sm text-white/45">
                  Decision registration opens after the meeting is held and the
                  secretary starts decision registration.
                </p>
              )}
              {selectedAgendaItem?.decisionStatus ? (
                <div className="mt-4 rounded-2xl border border-emerald-300/20 bg-emerald-400/10 p-4 text-sm">
                  <p className="font-semibold">
                    {getDropDownLabel(
                      decisionStatusOptions,
                      selectedAgendaItem.decisionStatus,
                    )}
                  </p>
                  <p className="mt-1 text-white/65">
                    {selectedAgendaItem.decisionText}
                  </p>
                  <p className="mt-2 text-xs text-white/45">
                    Final vote:{" "}
                    {selectedAgendaItem.finalVote ??
                      calculateFinalVote(selectedAgendaItem.votes)}
                  </p>
                </div>
              ) : (
                <p className="mt-4 text-sm text-white/45">
                  No decision registered for the selected agenda item.
                </p>
              )}
            </div>
          </section>

          <section className="gls rounded-2xl p-5">
            <h2 className="display text-lg font-semibold">Tasks</h2>
            {canEditTasks(details?.status) ? (
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
                <EmployeeSelect
                  employees={taskOwnerEmployees.length > 0 ? taskOwnerEmployees : employees}
                  value={taskForm.responsibleEmployeeId}
                  placeholder="Select responsible employee"
                  onChange={(value) =>
                    setTaskForm((current) => ({
                      ...current,
                      responsibleEmployeeId: value,
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
                      status: event.target.value as BoardProposalTaskStatus,
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
                  disabled={!selectedAgendaItem || Boolean(working)}
                >
                  Add task
                </button>
              </form>
            ) : (
              <p className="mt-4 rounded-2xl border border-white/10 bg-white/[0.04] p-4 text-sm text-white/45">
                Task creation opens during decision registration. Existing task
                progress remains editable during monitoring.
              </p>
            )}
            <div className="mt-4 space-y-2">
              {selectedTasks.map((task) => (
                <div
                  key={task.id}
                  draggable={canEditTasks(details?.status)}
                  onDragStart={() => setDraggedTaskId(task.id)}
                  onDragOver={(event: DragEvent<HTMLDivElement>) =>
                    canEditTasks(details?.status) && event.preventDefault()
                  }
                  onDrop={() =>
                    canEditTasks(details?.status) && void reorderTask(task.id)
                  }
                  className={`grid gap-2 rounded-2xl border border-white/10 bg-white/[0.05] p-4 text-sm md:grid-cols-[auto_1fr_auto] ${
                    canEditTasks(details?.status) ? "cursor-move" : ""
                  }`}
                >
                  <span className="text-white/35">
                    {canEditTasks(details?.status) ? "::" : "#"}
                  </span>
                  <span>
                    <span className="block font-semibold">{task.title}</span>
                    <span className="text-xs text-white/45">
                      {getEmployeeDisplayName(
                        task.responsibleEmployeeId,
                        taskOwnerEmployees,
                        employees,
                      )}{" "}
                      - Due{" "}
                      {new Date(task.dueDate).toLocaleDateString()}
                    </span>
                    {task.extendedDueDate && (
                      <span className="mt-1 block text-xs text-amber-100/75">
                        Extended to{" "}
                        {new Date(task.extendedDueDate).toLocaleDateString()}
                      </span>
                    )}
                    {task.description && (
                      <span className="mt-1 block text-white/55">
                        {task.description}
                      </span>
                    )}
                    {task.comment && (
                      <span className="mt-1 block text-white/45">
                        {task.comment}
                      </span>
                    )}
                  </span>
                  {canUpdateTaskStatus(details?.status) ? (
                    <select
                      className="field min-w-40 py-2 text-xs"
                      value={task.status}
                      disabled={Boolean(working)}
                      onChange={(event) =>
                        void updateTaskStatus(
                          task.id,
                          event.target.value as BoardProposalTaskStatus,
                        )
                      }
                    >
                      {taskStatusOptions.map((status) => (
                        <option key={status.code} value={status.code}>
                          {status.label}
                        </option>
                      ))}
                    </select>
                  ) : (
                    <span className="chip">
                      {getDropDownLabel(taskStatusOptions, task.status)}
                    </span>
                  )}
                </div>
              ))}
              {selectedTasks.length === 0 && (
                <p className="rounded-2xl border border-dashed border-white/15 p-4 text-sm text-white/45">
                  No tasks assigned for the selected agenda item.
                </p>
              )}
            </div>
          </section>
        </main>

        <aside className="space-y-5">
          <section className="gls rounded-2xl p-5">
            <h2 className="display text-lg font-semibold">Next action</h2>
            <p className="mt-2 text-sm text-white/50">
              {primaryAction?.note ?? "No available workflow action."}
            </p>
            <div className="mt-4 flex flex-col gap-3">
              {primaryAction ? (
                <button
                  type="button"
                  className="btn-primary"
                  disabled={
                    Boolean(working) ||
                    Boolean(primaryBlockReason) ||
                    Boolean(approvalBlockReason)
                  }
                  onClick={() =>
                    run(primaryAction.label, () =>
                      boardProposalApi.nextStep(id, primaryAction.action),
                    )
                  }
                >
                  {working ?? primaryAction.label}
                </button>
              ) : (
                <button type="button" className="btn-ghost" disabled>
                  No workflow action
                </button>
              )}
              {primaryBlockReason && (
                <p className="rounded-2xl border border-amber-300/20 bg-amber-400/10 p-3 text-sm text-amber-50">
                  {primaryBlockReason}
                </p>
              )}
              {approvalBlockReason && (
                <p className="rounded-2xl border border-amber-300/20 bg-amber-400/10 p-3 text-sm text-amber-50">
                  {approvalBlockReason}
                </p>
              )}
              {secondaryActions.map((action) => (
                <button
                  key={action.action}
                  type="button"
                  className={`btn-ghost ${
                    action.tone === "danger" ? "text-rose-100" : ""
                  }`}
                  disabled={Boolean(working) || Boolean(approvalBlockReason)}
                  onClick={() =>
                    run(action.label, () =>
                      boardProposalApi.nextStep(id, action.action),
                    )
                  }
                >
                  {action.label}
                </button>
              ))}
              {mayCancel && (
                <button
                  type="button"
                  className="btn-ghost text-rose-100"
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

          <section className="gls rounded-2xl p-5">
            <h2 className="display text-lg font-semibold">Meeting</h2>
            <dl className="mt-4 space-y-3 text-sm">
              <InfoRow label="Code" value={details?.meetingCode ?? "-"} />
              <InfoRow
                label="Type"
                value={getDropDownLabel([], details?.meetingType) || details?.meetingType || "-"}
              />
              <InfoRow
                label="Format"
                value={
                  getDropDownLabel([], details?.meetingFormat) ||
                  details?.meetingFormat ||
                  "-"
                }
              />
              <InfoRow
                label="Secretary"
                value={getEmployeeDisplayName(details?.secretaryEmployeeId, employees)}
              />
              <InfoRow
                label="Tasks"
                value={`${allTasks.length} total`}
              />
            </dl>
          </section>
        </aside>
      </div>
    </div>
  );
}

function EmployeeSelect({
  employees,
  value,
  placeholder,
  onChange,
  required,
}: {
  employees: EmployeeLookupItem[];
  value: string;
  placeholder: string;
  onChange: (value: string) => void;
  required?: boolean;
}) {
  return (
    <select
      className="field"
      value={value}
      onChange={(event) => onChange(event.target.value)}
      required={required}
    >
      <option value="" disabled>
        {placeholder}
      </option>
      {employees.map((employee) => (
        <option key={employee.id} value={employee.id}>
          {employeeLabel(employee)}
        </option>
      ))}
    </select>
  );
}

function getEmployeeDisplayName(
  employeeId: string | null | undefined,
  primary: EmployeeLookupItem[],
  fallback: EmployeeLookupItem[] = [],
) {
  if (!employeeId) return "-";

  const employee = [...primary, ...fallback].find((item) => item.id === employeeId);
  return employee ? employeeLabel(employee) : employeeId;
}

function InfoRow({ label, value }: { label: string; value: string }) {
  return (
    <div className="flex items-start justify-between gap-4">
      <dt className="text-white/40">{label}</dt>
      <dd className="text-right font-semibold text-white/75">{value}</dd>
    </div>
  );
}

function getAgendaAttachments(
  attachments: AttachmentDetails[],
  agendaItemId: number,
) {
  return attachments.filter(
    (attachment) =>
      attachment.section?.toLowerCase() === "agendaitem" &&
      attachment.sectionEntityId === agendaItemId,
  );
}

function isAgendaItemComplete(
  item: BoardProposalAgendaItemDetails,
  attachments: AttachmentDetails[],
) {
  return (
    Boolean(item.title?.trim()) &&
    Boolean(item.initiatorEmployeeId?.trim()) &&
    Boolean(item.responsibleBoardMemberEmployeeId?.trim()) &&
    Boolean(item.presenterEmployeeId?.trim()) &&
    Boolean(item.category?.trim()) &&
    item.order > 0 &&
    getAgendaAttachments(attachments, item.id).length > 0
  );
}

function allAgendaItemsComplete(
  details: BoardProposalRequestDetails,
  attachments: AttachmentDetails[],
) {
  return (
    details.agendaItems.length > 0 &&
    details.agendaItems.every((item) => isAgendaItemComplete(item, attachments))
  );
}

function getPrimaryBlockReason(
  details: BoardProposalRequestDetails | null,
  attachments: AttachmentDetails[],
) {
  if (!details) return null;

  switch (details.status) {
    case "AgendaPreparation":
    case "SecretaryReview":
      if (details.agendaItems.length === 0) {
        return "Add at least one agenda item before continuing.";
      }
      if (!allAgendaItemsComplete(details, attachments)) {
        return "Every agenda item needs title, owner, presenter, category, order, and material.";
      }
      return null;
    case "ChairpersonReview":
      if (details.agendaItems.length === 0 || details.agendaItems.some((x) => x.order <= 0)) {
        return "Every agenda item must have an agenda order.";
      }
      return null;
    case "Sent":
      if (new Date(details.meetingDate).getTime() > Date.now()) {
        return "The meeting date is still in the future.";
      }
      return null;
    case "DecisionsAndTasks": {
      if (details.agendaItems.some((item) => !item.decisionStatus)) {
        return "Register a final decision for every agenda item.";
      }
      const approvedWithoutTasks = details.agendaItems.some(
        (item) => item.decisionStatus === "Approved" && item.tasks.length === 0,
      );
      if (approvedWithoutTasks) {
        return "Every approved agenda item needs at least one follow-up task.";
      }
      return null;
    }
    case "DeadlineMonitoring": {
      const tasks = details.agendaItems.flatMap((item) => item.tasks);
      if (!tasks.every((task) => closedTaskStatuses.includes(task.status))) {
        return "All tasks must be completed, cancelled, or not applicable.";
      }
      return null;
    }
    default:
      return null;
  }
}

function getReadiness(
  details: BoardProposalRequestDetails | null,
  attachments: AttachmentDetails[],
) {
  if (!details) return [];

  const tasks = details.agendaItems.flatMap((item) => item.tasks);
  const approvedWithoutTasks = details.agendaItems.some(
    (item) => item.decisionStatus === "Approved" && item.tasks.length === 0,
  );

  return [
    {
      label: "At least one agenda item",
      done: details.agendaItems.length > 0,
    },
    {
      label: "All agenda items have required fields",
      done:
        details.agendaItems.length > 0 &&
        details.agendaItems.every(
          (item) =>
            Boolean(item.title?.trim()) &&
            Boolean(item.initiatorEmployeeId?.trim()) &&
            Boolean(item.responsibleBoardMemberEmployeeId?.trim()) &&
            Boolean(item.presenterEmployeeId?.trim()) &&
            Boolean(item.category?.trim()) &&
            item.order > 0,
        ),
    },
    {
      label: "All agenda items have materials",
      done:
        details.agendaItems.length > 0 &&
        details.agendaItems.every(
          (item) => getAgendaAttachments(attachments, item.id).length > 0,
        ),
    },
    {
      label: "All agenda items have decisions",
      done:
        details.agendaItems.length > 0 &&
        details.agendaItems.every((item) => Boolean(item.decisionStatus)),
    },
    {
      label: "Approved items have tasks",
      done: !approvedWithoutTasks,
    },
    {
      label: "All tasks are closeable",
      done:
        tasks.length === 0 ||
        tasks.every((task) => closedTaskStatuses.includes(task.status)),
    },
  ];
}

function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}
