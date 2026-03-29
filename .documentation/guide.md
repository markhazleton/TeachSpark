# TeachSpark Documentation Guide

This directory is the canonical home for repository documentation that should not live beside source code.

Documentation filenames under `.documentation/` follow a lowercase kebab-case convention. This guide normalizes discovery with categorized, clickable entries.

## Locations

- [.documentation/guides/](guides/) contains repository-level guides and implementation notes that remain operationally useful.
- [.documentation/teachspark-web/](teachspark-web/) contains application-specific guides for the ASP.NET MVC project.
- [.documentation/memory/](memory/) contains project governance and persistent planning context.
- [.documentation/copilot/](copilot/) contains Copilot-generated reports and workflow output.
- [.archive/README.md](../.archive/README.md) indexes historical markdown preserved for traceability.

## Root Files That Stay Put

- [README.md](../README.md) stays at the repository root as the main entry point.
- [CONTRIBUTING.md](../CONTRIBUTING.md) stays at the repository root for GitHub contributor workflows.
- [SECURITY.md](../SECURITY.md) stays at the repository root for GitHub security policy discovery.

## Repository Guides

- [Copilot Guidelines](guides/copilot-guidelines.md) explains repository-specific Copilot usage and prompt guidance.
- [Identity Implementation](guides/identity-implementation.md) documents the current ASP.NET Core Identity setup.
- [NET 10 Upgrade Instructions](guides/upgrade-to-net10-instructions.md) records the completed framework upgrade and troubleshooting steps.

## TeachSpark.Web Guides

- [Admin Area Documentation](teachspark-web/admin-area-documentation.md) describes the admin-area architecture and capabilities.
- [Build Guide](teachspark-web/build.md) explains the integrated .NET and webpack build workflow.
- [Build System Modernization](teachspark-web/build-system-documentation.md) records the frontend build modernization details.
- [DataTables Usage](teachspark-web/datatables-usage.md) covers DataTables setup and usage patterns.
- [Dynamic Model Selection](teachspark-web/dynamic-model-selection.md) documents LLM model-selection behavior and APIs.
- [LLM Integration Guide](teachspark-web/llm-integration-guide.md) explains LLM configuration and operational usage.
- [Prompt Templates Guide](teachspark-web/prompt-templates-guide.md) documents worksheet prompt-template behavior.
- [Serilog Implementation](teachspark-web/serilog-implementation.md) describes logging configuration and conventions.
- [Worksheet Database README](teachspark-web/worksheet-database-readme.md) documents worksheet-domain data modeling.