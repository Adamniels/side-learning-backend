SHELL := /bin/bash

.DEFAULT_GOAL := help

SOLUTION := SideLearning.slnx
API_PROJECT := src/SideLearning.Api/SideLearning.Api.csproj
INFRA_PROJECT := src/SideLearning.Infrastructure/SideLearning.Infrastructure.csproj
API_URL ?= http://localhost:5207

EMAIL ?= test.user@example.com
PASSWORD ?= Password1!
DISPLAY_NAME ?= Test User
REFRESH_TOKEN ?=
NAME ?=

.PHONY: help restore build test test-integration test-all clean format db-up db-down db-logs db-ps db-reset run run-http run-https watch migrate migration-add migration-remove register login refresh revoke health openapi-export contracts-ts-install contracts-ts-generate

help: ## Show all available commands
	@echo "Useful commands:" 
	@grep -E '^[a-zA-Z0-9_-]+:.*?## ' Makefile | sort | awk 'BEGIN {FS = ":.*?## "}; {printf "  %-18s %s\n", $$1, $$2}'

restore: ## Restore NuGet packages
	dotnet restore

build: ## Build entire solution
	dotnet build $(SOLUTION)

test: ## Run tests
	dotnet test $(SOLUTION)

test-integration: ## Run API integration tests (requires Docker)
	dotnet test tests/SideLearning.Api.IntegrationTests/SideLearning.Api.IntegrationTests.csproj

test-all: ## Run unit and integration tests
	dotnet test tests/SideLearning.Application.Tests/SideLearning.Application.Tests.csproj
	dotnet test tests/SideLearning.Api.IntegrationTests/SideLearning.Api.IntegrationTests.csproj

clean: ## Clean build outputs
	dotnet clean $(SOLUTION)

format: ## Format code (requires dotnet-format)
	dotnet format $(SOLUTION)

db-up: ## Start PostgreSQL via Docker Compose
	docker compose up -d

db-down: ## Stop PostgreSQL via Docker Compose
	docker compose down

db-logs: ## Tail PostgreSQL logs
	docker compose logs -f postgres

db-ps: ## Show Docker Compose services status
	docker compose ps

db-reset: ## Stop db and remove volume (destructive)
	docker compose down -v

run: run-http ## Run API on http profile (Development)

run-http: ## Run API with http launch profile (Development)
	ASPNETCORE_ENVIRONMENT=Development dotnet run --project $(API_PROJECT) --launch-profile http

run-https: ## Run API with https launch profile (Development)
	ASPNETCORE_ENVIRONMENT=Development dotnet run --project $(API_PROJECT) --launch-profile https

watch: ## Run API with dotnet watch (http profile)
	ASPNETCORE_ENVIRONMENT=Development dotnet watch --project $(API_PROJECT) run --launch-profile http

migrate: ## Apply migrations to database
	dotnet ef database update --project $(INFRA_PROJECT) --startup-project $(API_PROJECT)

migration-add: ## Add migration (use: make migration-add NAME=YourMigration)
	@if [ -z "$(NAME)" ]; then echo "Usage: make migration-add NAME=YourMigration"; exit 1; fi
	dotnet ef migrations add $(NAME) --project $(INFRA_PROJECT) --startup-project $(API_PROJECT) --output-dir Persistence/Migrations

migration-remove: ## Remove latest migration
	dotnet ef migrations remove --project $(INFRA_PROJECT) --startup-project $(API_PROJECT)

health: ## Check API health endpoint
	curl -sS -w "\nHTTP %{http_code}\n" $(API_URL)/health

register: ## Register test user (EMAIL, PASSWORD, DISPLAY_NAME)
	curl -sS -w "\nHTTP %{http_code}\n" -X POST $(API_URL)/api/v1/auth/register \
		-H "Content-Type: application/json" \
		-d '{"email":"$(EMAIL)","password":"$(PASSWORD)","displayName":"$(DISPLAY_NAME)"}'

login: ## Login user (EMAIL, PASSWORD)
	curl -sS -w "\nHTTP %{http_code}\n" -X POST $(API_URL)/api/v1/auth/login \
		-H "Content-Type: application/json" \
		-d '{"email":"$(EMAIL)","password":"$(PASSWORD)"}'

refresh: ## Refresh token (set REFRESH_TOKEN=...)
	@if [ -z "$(REFRESH_TOKEN)" ]; then echo "Usage: make refresh REFRESH_TOKEN=..."; exit 1; fi
	curl -sS -w "\nHTTP %{http_code}\n" -X POST $(API_URL)/api/v1/auth/refresh \
		-H "Content-Type: application/json" \
		-d '{"refreshToken":"$(REFRESH_TOKEN)"}'

revoke: ## Revoke refresh token (set REFRESH_TOKEN=...)
	@if [ -z "$(REFRESH_TOKEN)" ]; then echo "Usage: make revoke REFRESH_TOKEN=..."; exit 1; fi
	curl -sS -w "\nHTTP %{http_code}\n" -X POST $(API_URL)/api/v1/auth/revoke \
		-H "Content-Type: application/json" \
		-d '{"refreshToken":"$(REFRESH_TOKEN)"}'

openapi-export: ## Export swagger json to contracts/openapi/openapi.json
	@mkdir -p contracts/openapi
	@ASPNETCORE_ENVIRONMENT=Development dotnet run --project $(API_PROJECT) --launch-profile http --no-build > /tmp/sidelearning-openapi.log 2>&1 & \
	PID=$$!; \
	for i in {1..40}; do \
		curl -fsS $(API_URL)/swagger/v1/swagger.json -o contracts/openapi/openapi.json && break; \
		sleep 1; \
	done; \
	kill $$PID >/dev/null 2>&1 || true; \
	wait $$PID >/dev/null 2>&1 || true; \
	test -f contracts/openapi/openapi.json

contracts-ts-install: ## Install TypeScript contracts generator dependencies
	npm --prefix contracts/typescript install

contracts-ts-generate: contracts-ts-install openapi-export ## Generate TS client+types from OpenAPI
	npm --prefix contracts/typescript run generate
