# Start each API, then download the runtime OpenAPI JSON produced by MapOpenApi().
# Event API:   https://localhost:7001/openapi/v1.json
# Ticket API:  https://localhost:7002/openapi/v1.json
New-Item -ItemType Directory -Force -Path .\openapi | Out-Null
Invoke-WebRequest -Uri https://localhost:7001/openapi/v1.json -OutFile .\openapi\event-management.openapi.json -SkipCertificateCheck
Invoke-WebRequest -Uri https://localhost:7002/openapi/v1.json -OutFile .\openapi	icket-management.openapi.json -SkipCertificateCheck
