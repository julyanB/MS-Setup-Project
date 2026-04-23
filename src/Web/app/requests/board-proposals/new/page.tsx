"use client";

import Link from "next/link";
import { useState } from "react";
import { ProtectedRoute } from "@/components/ProtectedRoute";

const requiredDocuments = [
  "Proposal to Management Board",
  "Financial data",
  "Credit risk opinion",
  "Legal opinion",
  "Bank security opinion",
  "Compliance opinion",
  "SIR detailed description",
];

export default function NewBoardProposalPage() {
  return (
    <ProtectedRoute>
      <NewBoardProposalContent />
    </ProtectedRoute>
  );
}

function NewBoardProposalContent() {
  const [category, setCategory] = useState("Credit");
  const [uploaded, setUploaded] = useState<string[]>([
    "Proposal to Management Board",
    "Financial data",
  ]);

  function toggleDocument(name: string) {
    setUploaded((current) =>
      current.includes(name)
        ? current.filter((item) => item !== name)
        : [...current, name],
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
            Create proposal
          </h1>
          <p className="mt-1 max-w-2xl text-sm text-white/55">
            Prepare a request for secretary review before it reaches the
            chairperson.
          </p>
        </div>
        <Link href="/requests" className="btn-ghost">
          Back to requests
        </Link>
      </header>

      <div className="grid gap-6 lg:grid-cols-[minmax(0,1fr)_360px]">
        <form className="gls space-y-5 rounded-2xl p-6">
          <div className="grid gap-4 md:grid-cols-2">
            <div className="md:col-span-2">
              <label className="label">Title</label>
              <input
                className="field"
                placeholder="Credit deal - Alfa OOD"
                maxLength={1000}
              />
            </div>

            <div>
              <label className="label">Category</label>
              <select
                className="field"
                value={category}
                onChange={(event) => setCategory(event.target.value)}
              >
                <option>Credit</option>
                <option>Business</option>
                <option>Regulatory</option>
                <option>Expense</option>
                <option>Organizational</option>
                <option>Other</option>
              </select>
            </div>

            <div>
              <label className="label">Priority</label>
              <select className="field" defaultValue="Medium">
                <option>High</option>
                <option>Medium</option>
                <option>Low</option>
              </select>
            </div>

            <div>
              <label className="label">Initiator</label>
              <select className="field" defaultValue="I. Georgiev">
                <option>I. Georgiev</option>
                <option>M. Ivanova</option>
                <option>T. Koleva</option>
              </select>
            </div>

            <div>
              <label className="label">Responsible board member</label>
              <select className="field" defaultValue="P. Dimitrov">
                <option>P. Dimitrov</option>
                <option>V. Kostova</option>
                <option>R. Toleva</option>
              </select>
            </div>

            <div>
              <label className="label">Presenter</label>
              <select className="field" defaultValue="I. Georgiev">
                <option>I. Georgiev</option>
                <option>M. Ivanova</option>
                <option>T. Koleva</option>
              </select>
            </div>

            <div>
              <label className="label">Department</label>
              <input className="field" placeholder="Corporate banking" />
            </div>
          </div>

          <div>
            <label className="label">Description</label>
            <textarea
              className="field min-h-36 resize-y"
              maxLength={1500}
              placeholder="Short description for secretary and chairperson review"
            />
          </div>

          <div className="flex flex-wrap gap-3">
            <button type="button" className="btn-primary">
              Save draft
            </button>
            <button type="button" className="btn-ghost">
              Submit for secretary review
            </button>
          </div>
        </form>

        <aside className="space-y-5">
          <section className="gls rounded-2xl p-5">
            <h2 className="display text-lg font-semibold">Workflow</h2>
            <div className="mt-4 space-y-3">
              {[
                ["Initiator", "Draft in progress"],
                ["Secretary", "Completeness review"],
                ["Chairperson", "Final agenda decision"],
              ].map(([role, note], index) => (
                <div key={role} className="flex gap-3">
                  <span className="grid h-8 w-8 place-items-center rounded-xl border border-white/15 bg-white/10 text-sm font-bold">
                    {index + 1}
                  </span>
                  <div>
                    <p className="text-sm font-semibold">{role}</p>
                    <p className="text-xs text-white/45">{note}</p>
                  </div>
                </div>
              ))}
            </div>
          </section>

          <section className="gls rounded-2xl p-5">
            <div className="flex items-center justify-between gap-3">
              <h2 className="display text-lg font-semibold">Materials</h2>
              <span className="chip">
                {uploaded.length} / {requiredDocuments.length}
              </span>
            </div>
            <p className="mt-1 text-xs text-white/45">
              Category: {category}
            </p>

            <div className="mt-4 space-y-2">
              {requiredDocuments.map((document) => {
                const isUploaded = uploaded.includes(document);
                return (
                  <button
                    key={document}
                    type="button"
                    onClick={() => toggleDocument(document)}
                    className={`flex w-full items-center gap-3 rounded-xl border px-3 py-2.5 text-left text-sm transition ${
                      isUploaded
                        ? "border-emerald-300/30 bg-emerald-400/10 text-emerald-100"
                        : "border-white/10 bg-white/[0.05] text-white/65 hover:bg-white/[0.1]"
                    }`}
                  >
                    <span className="grid h-5 w-5 place-items-center rounded-md border border-current text-[11px]">
                      {isUploaded ? "OK" : "!"}
                    </span>
                    {document}
                  </button>
                );
              })}
            </div>
          </section>
        </aside>
      </div>
    </div>
  );
}
