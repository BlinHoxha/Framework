# AI extraction migration

AI now belongs to the independent sibling AI-Framework solution. General Framework projects have no AI project/package references, endpoints or configuration. Original source snapshots (including pre-existing workspace edits) were retained under `.artifacts/ai-extraction-originals`; earlier README snapshots are under `.artifacts` too.

| Before | After |
| --- | --- |
| BuildingBlocks.AI.Core/Local/Azure | AIFramework.Core/Local/Azure in AI-Framework |
| Framework.Application.AI.Services | AIFramework.Application.Services |
| Framework.Application.AI.DocumentProfiles | AIFramework.Application.DocumentProfiles |
| Framework.Contracts.AI | AIFramework.Contracts |
| AddAiInfrastructure | Host selects AddLocalAi, AddAzureAi or AddBedrockChat |
| AI registration inside AddApplication | Explicit AddAiApplication from AIFramework.Application |
| Framework AI endpoints | Independent AIFramework.SampleApi |
| AI Search schema/bootstrap/deployment | AI-Framework/infrastructure |

HTTP JSON shapes remain compatible; source namespaces/assembly references intentionally change. No compatibility AI libraries remain in Framework.

MixPilot keeps owner-scoped evidence, citations, AI feature flags and deterministic planning. Its provider references now point directly to AI-Framework. BusinessSuite has no operational AI feature to migrate; its explicit Core reference points to the independent library for future use.

Use sibling project references during workspace development or matching private NuGet versions for independent releases. Do not mix project and package references for one assembly.

Cloud source extraction does not migrate cloud state. Read the AI infrastructure migration guidance before transferring Terraform state or Bicep resource ownership. No cloud apply, deletion or state migration was performed.
