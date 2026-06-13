import axiosClient from './axiosClient';
import { type DashboardAnalyticsResponse } from '../types/analytics'; 

const analyticsApi = {
  getSummary: (): Promise<DashboardAnalyticsResponse> => {
    return axiosClient.get('/analytics/summary');
  },
};

export default analyticsApi;