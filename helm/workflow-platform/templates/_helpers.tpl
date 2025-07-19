{{/*
Expand the name of the chart.
*/}}
{{- define "workflow-platform.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Create a default fully qualified app name.
*/}}
{{- define "workflow-platform.fullname" -}}
{{- if .Values.fullnameOverride }}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- $name := default .Chart.Name .Values.nameOverride }}
{{- if contains $name .Release.Name }}
{{- .Release.Name | trunc 63 | trimSuffix "-" }}
{{- else }}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" }}
{{- end }}
{{- end }}
{{- end }}

{{/*
Create chart name and version as used by the chart label.
*/}}
{{- define "workflow-platform.chart" -}}
{{- printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{- end }}

{{/*
Common labels
*/}}
{{- define "workflow-platform.labels" -}}
helm.sh/chart: {{ include "workflow-platform.chart" . }}
{{ include "workflow-platform.selectorLabels" . }}
{{- if .Chart.AppVersion }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
{{- end }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
{{- if .Values.commonLabels }}
{{ toYaml .Values.commonLabels }}
{{- end }}
{{- end }}

{{/*
Selector labels
*/}}
{{- define "workflow-platform.selectorLabels" -}}
app.kubernetes.io/name: {{ include "workflow-platform.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end }}

{{/*
API selector labels
*/}}
{{- define "workflow-platform.api.selectorLabels" -}}
app.kubernetes.io/name: {{ include "workflow-platform.name" . }}-api
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: api
{{- end }}

{{/*
Frontend selector labels
*/}}
{{- define "workflow-platform.frontend.selectorLabels" -}}
app.kubernetes.io/name: {{ include "workflow-platform.name" . }}-frontend
app.kubernetes.io/instance: {{ .Release.Name }}
app.kubernetes.io/component: frontend
{{- end }}

{{/*
Create the name of the service account to use
*/}}
{{- define "workflow-platform.serviceAccountName" -}}
{{- if .Values.serviceAccount.create }}
{{- default (include "workflow-platform.fullname" .) .Values.serviceAccount.name }}
{{- else }}
{{- default "default" .Values.serviceAccount.name }}
{{- end }}
{{- end }}

{{/*
Generate image repository with global registry override
*/}}
{{- define "workflow-platform.api.image" -}}
{{- if .Values.global.imageRegistry }}
{{- printf "%s/%s" .Values.global.imageRegistry (.Values.api.image.repository | replace "ghcr.io/" "" | replace "docker.io/" "") }}
{{- else }}
{{- .Values.api.image.repository }}
{{- end }}
{{- end }}

{{- define "workflow-platform.frontend.image" -}}
{{- if .Values.global.imageRegistry }}
{{- printf "%s/%s" .Values.global.imageRegistry (.Values.frontend.image.repository | replace "ghcr.io/" "" | replace "docker.io/" "") }}
{{- else }}
{{- .Values.frontend.image.repository }}
{{- end }}
{{- end }}

{{/*
Database connection string
*/}}
{{- define "workflow-platform.database.connectionString" -}}
{{- if .Values.database.existingSecret }}
{{- printf "valueFrom:\n  secretKeyRef:\n    name: %s\n    key: %s" .Values.database.existingSecret .Values.database.secretKey | nindent 2 }}
{{- else }}
{{- printf "Host=%s;Port=%d;Database=%s;Username=%s;Password=%s" .Values.database.host (.Values.database.port | int) .Values.database.name .Values.database.username .Values.database.password }}
{{- end }}
{{- end }}

{{/*
Redis connection string
*/}}
{{- define "workflow-platform.redis.connectionString" -}}
{{- if .Values.redis.existingSecret }}
{{- printf "valueFrom:\n  secretKeyRef:\n    name: %s\n    key: %s" .Values.redis.existingSecret .Values.redis.secretKey | nindent 2 }}
{{- else }}
{{- printf "%s:%d" .Values.redis.host (.Values.redis.port | int) }}
{{- end }}
{{- end }}

{{/*
RabbitMQ connection string
*/}}
{{- define "workflow-platform.rabbitmq.connectionString" -}}
{{- if .Values.rabbitmq.existingSecret }}
{{- printf "valueFrom:\n  secretKeyRef:\n    name: %s\n    key: %s" .Values.rabbitmq.existingSecret .Values.rabbitmq.secretKey | nindent 2 }}
{{- else }}
{{- printf "amqp://%s:%s@%s:%d/" .Values.rabbitmq.username .Values.rabbitmq.password .Values.rabbitmq.host (.Values.rabbitmq.port | int) }}
{{- end }}
{{- end }}
