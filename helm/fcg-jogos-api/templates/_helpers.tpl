{{- define "fcg-jogos-api.name" -}}
fcg-jogos-api
{{- end -}}

{{- define "fcg-jogos-api.fullname" -}}
{{ .Release.Name }}-{{ include "fcg-jogos-api.name" . }}
{{- end -}}
