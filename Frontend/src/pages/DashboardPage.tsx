import React, { useEffect, useMemo, useState } from 'react';
import {
  Bar,
  BarChart,
  CartesianGrid,
  Legend,
  Line,
  LineChart,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts';

import analyticsApi from '../api/analyticsApi';
import {
  type DashboardAnalyticsResponse,
  type LabeledCount,
  type TrainingRunHistoryItem,
} from '../types/analytics';

const DashboardPage: React.FC = () => {
  const [data, setData] = useState<DashboardAnalyticsResponse | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchAnalytics = async () => {
      try {
        setLoading(true);
        const response: any = await analyticsApi.getSummary();
        const actualData = response && response.data ? response.data : response;
        setData(actualData);
      } catch (err: any) {
        console.error('Failed to fetch analytics:', err);
        setError('Unable to load dashboard data right now.');
      } finally {
        setLoading(false);
      }
    };

    void fetchAnalytics();
  }, []);

  const categoryChartData = useMemo(
    () =>
      Object.keys(data?.messagesByCategory || {}).map((key) => ({
        name: key,
        count: data?.messagesByCategory?.[key] || 0,
      })),
    [data],
  );

  const trendChartData = useMemo(
    () =>
      (data?.weeklyTrends || []).map((item) => {
        const parts = item.date.split('-');
        return {
          ...item,
          displayDate: parts.length === 3 ? `${parts[2]}/${parts[1]}` : item.date,
        };
      }),
    [data],
  );

  if (loading) {
    return (
      <div className="flex min-h-screen flex-col items-center justify-center gap-3 text-gray-500">
        <svg className="h-8 w-8 animate-spin text-indigo-600" fill="none" viewBox="0 0 24 24">
          <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
          <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
        </svg>
        <p className="text-sm font-medium text-gray-600">Loading dashboard data...</p>
      </div>
    );
  }

  if (error || !data) {
    return (
      <div className="flex min-h-screen items-center justify-center font-medium text-red-500">
        {error || 'A system error occurred.'}
      </div>
    );
  }

  const reviewAnalytics = data.reviewAnalytics;
  const hasConfidenceData = reviewAnalytics.confidenceBuckets.some((item) => item.count > 0);
  const hasTopIntentData = reviewAnalytics.topReviewIntents.some((item) => item.count > 0);

  return (
    <div className="min-h-screen w-full overflow-x-hidden bg-gray-50 p-6 text-gray-800">
      <div className="mb-6 flex items-center space-x-2">
        <h1 className="text-2xl font-bold tracking-tight">AI Operations Dashboard</h1>
      </div>

      <div className="mb-8 grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-4">
        <MetricCard label="Conversations" value={data.totalConversations} tone="indigo" />
        <MetricCard label="Messages" value={data.totalMessages} tone="emerald" />
        <MetricCard label="Products" value={data.totalProducts} tone="amber" />
        <MetricCard label="Policies" value={data.totalSupportPolicies} tone="rose" />
      </div>

      <div className="mb-8 grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-4">
        <MetricCard label="Documents" value={data.totalDocuments} tone="sky" />
        <MetricCard label="Evaluations" value={data.totalChatEvaluations} tone="violet" />
        <MetricCard label="Need review" value={data.totalNeedsReview} tone="red" />
        <MetricCard label="Avg confidence" value={`${data.averageConfidenceScore}%`} tone="teal" />
      </div>

      <div className="mb-8 grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-4">
        <MetricCard label="Low confidence" value={reviewAnalytics.lowConfidenceCount} tone="red" />
        <MetricCard label="Knowledge gaps" value={reviewAnalytics.knowledgeGapCount} tone="amber" />
        <MetricCard label="Ready to train" value={reviewAnalytics.readyForTrainingCount} tone="indigo" />
        <MetricCard label="Training runs" value={reviewAnalytics.totalTrainingRuns} tone="emerald" />
      </div>

      <div className="grid grid-cols-1 gap-8 xl:grid-cols-2">
        <ChartCard title="Conversation trend (last 7 days)">
          <ResponsiveContainer width="100%" height={320}>
            <LineChart data={trendChartData} margin={{ top: 10, right: 12, left: -20, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
              <XAxis dataKey="displayDate" stroke="#9ca3af" fontSize={11} tickLine={false} />
              <YAxis allowDecimals={false} stroke="#9ca3af" fontSize={11} tickLine={false} />
              <Tooltip />
              <Legend iconType="circle" wrapperStyle={{ fontSize: '12px' }} />
              <Line
                type="monotone"
                dataKey="count"
                name="New conversations"
                stroke="#4f46e5"
                strokeWidth={3}
                dot={{ r: 4 }}
                activeDot={{ r: 6 }}
              />
            </LineChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Messages by category">
          <ResponsiveContainer width="100%" height={320}>
            <BarChart data={categoryChartData} margin={{ top: 10, right: 12, left: -20, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
              <XAxis dataKey="name" stroke="#9ca3af" fontSize={11} tickLine={false} />
              <YAxis allowDecimals={false} stroke="#9ca3af" fontSize={11} tickLine={false} />
              <Tooltip />
              <Legend iconType="square" wrapperStyle={{ fontSize: '12px' }} />
              <Bar dataKey="count" fill="#ec4899" radius={[4, 4, 0, 0]} barSize={36} name="Messages" />
            </BarChart>
          </ResponsiveContainer>
        </ChartCard>

        <ChartCard title="Review confidence buckets">
          {hasConfidenceData ? (
            <ResponsiveContainer width="100%" height={320}>
              <BarChart data={reviewAnalytics.confidenceBuckets} margin={{ top: 10, right: 12, left: -20, bottom: 8 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
                <XAxis dataKey="label" stroke="#9ca3af" fontSize={11} tickLine={false} />
                <YAxis allowDecimals={false} stroke="#9ca3af" fontSize={11} tickLine={false} />
                <Tooltip />
                <Legend iconType="square" wrapperStyle={{ fontSize: '12px' }} />
                <Bar dataKey="count" fill="#0f766e" radius={[4, 4, 0, 0]} barSize={42} name="Reviews" />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <EmptyChartState message="Confidence bucket data will appear after review items are collected." />
          )}
        </ChartCard>

        <ChartCard title="Top review intents">
          {hasTopIntentData ? (
            <ResponsiveContainer width="100%" height={320}>
              <BarChart data={reviewAnalytics.topReviewIntents} margin={{ top: 10, right: 12, left: -8, bottom: 36 }}>
                <CartesianGrid strokeDasharray="3 3" stroke="#f3f4f6" />
                <XAxis
                  dataKey="label"
                  stroke="#9ca3af"
                  fontSize={11}
                  tickLine={false}
                  interval={0}
                  angle={-18}
                  textAnchor="end"
                  height={72}
                />
                <YAxis allowDecimals={false} stroke="#9ca3af" fontSize={11} tickLine={false} />
                <Tooltip />
                <Legend iconType="square" wrapperStyle={{ fontSize: '12px' }} />
                <Bar dataKey="count" fill="#7c3aed" radius={[4, 4, 0, 0]} barSize={42} name="Reviews" />
              </BarChart>
            </ResponsiveContainer>
          ) : (
            <EmptyChartState message="Top review intents will appear after low-confidence reviews are grouped." />
          )}
        </ChartCard>
      </div>

      <div className="mt-8 grid grid-cols-1 gap-8 xl:grid-cols-[1.2fr_0.8fr]">
        <section className="rounded-xl border border-gray-100 bg-white p-6 shadow-xs">
          <div className="mb-4 flex items-center justify-between gap-3">
            <div>
              <h3 className="text-md font-bold text-gray-800">Training run history</h3>
              <p className="mt-1 text-sm text-gray-500">Latest model training attempts and outcomes.</p>
            </div>
          </div>

          {data.trainingRunHistory.length === 0 ? (
            <div className="rounded-lg border border-dashed border-gray-200 bg-gray-50 px-4 py-6 text-sm text-gray-500">
              No training run has been recorded yet.
            </div>
          ) : (
            <div className="overflow-x-auto">
              <table className="min-w-full table-fixed text-left text-sm">
                <thead className="text-xs uppercase tracking-wide text-gray-400">
                  <tr>
                    <th className="w-24 pb-3 pr-4 font-semibold">Status</th>
                    <th className="pb-3 pr-4 font-semibold">Run</th>
                    <th className="w-28 pb-3 pr-4 font-semibold">Dataset</th>
                    <th className="w-28 pb-3 pr-4 font-semibold">Accuracy</th>
                    <th className="w-32 pb-3 pr-4 font-semibold">Model</th>
                    <th className="w-40 pb-3 font-semibold">Updated</th>
                  </tr>
                </thead>
                <tbody>
                  {data.trainingRunHistory.map((run) => (
                    <TrainingRunRow key={run.id} run={run} />
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </section>

        <section className="rounded-xl border border-gray-100 bg-white p-6 shadow-xs">
          <div className="mb-4">
            <h3 className="text-md font-bold text-gray-800">Review analytics</h3>
            <p className="mt-1 text-sm text-gray-500">How review items are flowing into training and knowledge updates.</p>
          </div>

          <div className="space-y-3">
            <SummaryRow label="Pending review" value={findCount(reviewAnalytics.reviewOutcomes, 'Pending review')} />
            <SummaryRow label="Knowledge gaps" value={findCount(reviewAnalytics.reviewOutcomes, 'Knowledge gaps')} />
            <SummaryRow label="Ready to train" value={reviewAnalytics.readyForTrainingCount} />
            <SummaryRow label="Trained examples" value={reviewAnalytics.trainedExampleCount} />
            <SummaryRow label="Low confidence reviews" value={reviewAnalytics.lowConfidenceCount} />
          </div>
        </section>
      </div>
    </div>
  );
};

function MetricCard({
  label,
  value,
  tone,
}: {
  label: string;
  value: string | number;
  tone: 'indigo' | 'emerald' | 'amber' | 'rose' | 'sky' | 'violet' | 'red' | 'teal';
}) {
  const tones: Record<string, string> = {
    indigo: 'bg-indigo-50 text-indigo-700',
    emerald: 'bg-emerald-50 text-emerald-700',
    amber: 'bg-amber-50 text-amber-700',
    rose: 'bg-rose-50 text-rose-700',
    sky: 'bg-sky-50 text-sky-700',
    violet: 'bg-violet-50 text-violet-700',
    red: 'bg-red-50 text-red-700',
    teal: 'bg-teal-50 text-teal-700',
  };

  return (
    <div className="flex items-center justify-between rounded-xl border border-gray-100 bg-white p-6 shadow-xs">
      <div>
        <p className="text-xs font-semibold uppercase tracking-wider text-gray-400">{label}</p>
        <p className="mt-1 text-3xl font-extrabold text-gray-900">{value}</p>
      </div>
      <div className={`rounded-xl px-3 py-2 text-sm font-bold ${tones[tone]}`}>
        {label.slice(0, 2).toUpperCase()}
      </div>
    </div>
  );
}

function ChartCard({
  title,
  children,
}: {
  title: string;
  children: React.ReactNode;
}) {
  return (
    <div className="flex min-w-0 flex-col rounded-xl border border-gray-100 bg-white p-6 shadow-xs">
      <h3 className="mb-4 text-left text-md font-bold text-gray-700">{title}</h3>
      <div className="h-[340px] min-h-[340px] w-full">{children}</div>
    </div>
  );
}

function EmptyChartState({ message }: { message: string }) {
  return (
    <div className="flex h-full items-center justify-center rounded-lg border border-dashed border-gray-200 bg-gray-50 px-6 text-center text-sm text-gray-500">
      {message}
    </div>
  );
}

function SummaryRow({
  label,
  value,
}: {
  label: string;
  value: number;
}) {
  return (
    <div className="flex items-center justify-between rounded-lg border border-gray-100 bg-gray-50 px-4 py-3">
      <span className="text-sm font-medium text-gray-600">{label}</span>
      <span className="text-base font-semibold text-gray-900">{value}</span>
    </div>
  );
}

function TrainingRunRow({ run }: { run: TrainingRunHistoryItem }) {
  return (
    <tr className="border-t border-gray-100 align-top text-gray-700">
      <td className="py-3 pr-4">
        <StatusBadge status={run.status} />
      </td>
      <td className="py-3 pr-4">
        <div className="space-y-1">
          <div className="font-medium text-gray-900">{run.message || 'Training run'}</div>
          {run.error ? <div className="text-xs text-red-500">{run.error}</div> : null}
        </div>
      </td>
      <td className="py-3 pr-4 text-sm text-gray-600">
        <div>{run.datasetSize} total</div>
        <div className="text-xs text-gray-400">{run.reviewedExampleCount} reviewed</div>
      </td>
      <td className="py-3 pr-4 text-sm text-gray-600">{formatAccuracy(run.accuracy)}</td>
      <td className="py-3 pr-4 text-sm text-gray-600">
        <div>v{run.modelVersion || 0}</div>
        <div className="text-xs text-gray-400">{run.bestModelName || '-'}</div>
      </td>
      <td className="py-3 text-sm text-gray-600">
        <div>{formatDateTime(run.completedAt || run.updatedAt)}</div>
        <div className="text-xs text-gray-400">
          {run.startedAt ? `Started ${formatDateTime(run.startedAt)}` : 'No start time'}
        </div>
      </td>
    </tr>
  );
}

function StatusBadge({ status }: { status: string }) {
  const normalized = status.toLowerCase();
  const tone =
    normalized === 'succeeded'
      ? 'bg-emerald-50 text-emerald-700'
      : normalized === 'failed'
        ? 'bg-red-50 text-red-700'
        : 'bg-amber-50 text-amber-700';

  return (
    <span className={`inline-flex rounded-full px-2.5 py-1 text-xs font-semibold capitalize ${tone}`}>
      {status}
    </span>
  );
}

function findCount(items: LabeledCount[], label: string) {
  return items.find((item) => item.label === label)?.count ?? 0;
}

function formatAccuracy(value: number) {
  if (!Number.isFinite(value) || value <= 0) {
    return '-';
  }

  return `${(value * 100).toFixed(1)}%`;
}

function formatDateTime(value?: string | null) {
  if (!value) {
    return '-';
  }

  const date = new Date(value);
  return date.toLocaleString();
}

export default DashboardPage;
