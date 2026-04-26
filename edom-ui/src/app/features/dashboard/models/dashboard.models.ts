import { Type } from '@angular/core';

export type DashboardWidgetState = 'active' | 'removed';

export interface DashboardWidgetRuntime {
  loading?: boolean;
  error?: string | null;
  empty?: boolean;
}

export interface DashboardWidgetInstance {
  id: string;
  type: string;
  title?: string;
  x: number;
  y: number;
  w: number;
  h: number;
  config?: Record<string, unknown>;
  datasource?: Record<string, unknown>;
  state: DashboardWidgetState;
  children?: DashboardWidgetInstance[];
}

export interface DashboardLayout {
  schemaVersion: number;
  updatedAt: string;
  widgets: DashboardWidgetInstance[];
}

export interface DashboardWidgetComponentInputs {
  [key: string]: unknown;
  config: Record<string, unknown>;
  datasource: Record<string, unknown>;
  runtime: DashboardWidgetRuntime;
  depth: number;
}

export type DashboardWidgetLoader = () => Promise<Type<unknown>>;

export interface DashboardWidgetMetadata {
  type: string;
  title: string;
  icon: string;
  description: string;
  minW: number;
  minH: number;
  maxW?: number;
  maxH?: number;
  defaultW: number;
  defaultH: number;
  supportsChildren?: boolean;
  loadComponent: DashboardWidgetLoader;
}

export interface DashboardWidgetProtocol {
  config: Record<string, unknown>;
  datasource: Record<string, unknown>;
  runtime: DashboardWidgetRuntime;
  depth: number;
  refresh?: () => void;
}
