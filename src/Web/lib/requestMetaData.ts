import { api } from "./api";

export type RequestMetaDataItem = {
  id: number;
  vId: string;
  requestType: string;
  status: string;
  createdBy: string;
  modifiedBy: string;
  createdAt: string;
  updatedAt: string;
  seen: boolean;
  additionalJsonData?: string | null;
};

export type SearchRequestMetaDataResponse = {
  page: number;
  pageSize: number;
  totalCount: number;
  items: RequestMetaDataItem[];
};

export type SearchRequestMetaDataParams = {
  page?: number;
  pageSize?: number;
  requestType?: string;
  status?: string;
  onlyUnseen?: boolean;
  createdBy?: string;
  assignedToMe?: boolean;
  assignedToMyRole?: boolean;
};

export type BoardProposalMetaData = {
  meetingCode?: string;
  meetingDate?: string;
  meetingType?: string;
  meetingFormat?: string;
  secretaryEmployeeId?: string;
};

export const requestMetaDataApi = {
  search: (params: SearchRequestMetaDataParams = {}) => {
    const query = new URLSearchParams();
    if (params.page !== undefined) query.set("page", String(params.page));
    if (params.pageSize !== undefined)
      query.set("pageSize", String(params.pageSize));
    if (params.requestType) query.set("requestType", params.requestType);
    if (params.status) query.set("status", params.status);
    if (params.onlyUnseen) query.set("onlyUnseen", "true");
    if (params.createdBy) query.set("createdBy", params.createdBy);
    if (params.assignedToMe) query.set("assignedToMe", "true");
    if (params.assignedToMyRole) query.set("assignedToMyRole", "true");

    const qs = query.toString();
    return api.get<SearchRequestMetaDataResponse>(
      `/request-metadata${qs ? `?${qs}` : ""}`,
    );
  },

  markSeen: (requestType: string, id: number) =>
    api.patch<void>(`/request-metadata/${requestType}/${id}/seen`),
};

export function parseBoardProposalMetaData(
  raw: string | null | undefined,
): BoardProposalMetaData | null {
  if (!raw) return null;
  try {
    return JSON.parse(raw) as BoardProposalMetaData;
  } catch {
    return null;
  }
}
