# AI Meeting Notes Application
## Software Requirements Specification (SRS), System Design & Development Roadmap
**Version:** 2.0.0 | **Status:** Draft | **Date:** 2025
**Backend Stack:** FastAPI (Python) + .NET MAUI (Mobile)

---

## Table of Contents

1. [Product Vision](#1-product-vision)
2. [Feature Breakdown](#2-feature-breakdown)
3. [System Architecture](#3-system-architecture)
4. [Database Design](#4-database-design)
5. [Backend Design](#5-backend-design)
6. [API Design](#6-api-design)
7. [AI Design](#7-ai-design)
8. [Mobile Design](#8-mobile-design)
9. [Security Design](#9-security-design)
10. [Deployment Architecture](#10-deployment-architecture)
11. [Testing Strategy](#11-testing-strategy)
12. [Development Roadmap](#12-development-roadmap)
13. [Copilot Integration Guide](#13-copilot-integration-guide)

---

## 1. Product Vision

### 1.1 Problem Statement

Professionals lose hundreds of hours annually re-reading meeting notes, following up on
action items, and searching for past decisions. Existing tools require manual note-taking,
miss critical context, and offer no intelligent retrieval. There is no solution that combines
**automatic transcription**, **AI-driven summarisation**, and **conversational retrieval**
in a single mobile-first experience.

### 1.2 User Personas

| Persona | Role | Pain Points | Goals |
|---|---|---|---|
| **Arjun** | Engineering Manager | Can't track action items across 10+ meetings/week | Auto-extracted tasks synced to Jira |
| **Priya** | Product Manager | Spends 2 hrs/week writing meeting summaries | Instant AI summaries to share |
| **David** | Consultant | Needs to reference client decisions months later | Semantic search across all past meetings |
| **Sarah** | C-Suite Executive | Misses context in back-to-back meetings | 60-second executive briefings |

### 1.3 User Stories

#### Authentication
- `US-001` As a user, I can register with email and password so I can access the app.
- `US-002` As a user, I can log in and receive a JWT so I stay authenticated.
- `US-003` As a user, I can reset my password via email so I regain access if locked out.

#### Meeting Management
- `US-010` As a user, I can create a meeting with a title, date, and participants.
- `US-011` As a user, I can record audio directly from my phone during a live meeting.
- `US-012` As a user, I can upload an existing audio file (MP3, WAV, M4A).
- `US-013` As a user, I can view a list of all past meetings sorted by date.
- `US-014` As a user, I can delete a meeting and all associated data.

#### AI Features
- `US-020` As a user, I can trigger transcription of recorded audio.
- `US-021` As a user, I can view a full transcript with timestamps.
- `US-022` As a user, I can generate an executive summary of any meeting.
- `US-023` As a user, I can view extracted action items with owner, priority, and due date.
- `US-024` As a user, I can view key decisions extracted from the meeting.
- `US-025` As a user, I can chat with a meeting using natural language.

#### Search & Export
- `US-030` As a user, I can search meetings by keyword across transcripts and summaries.
- `US-031` As a user, I can export a meeting as PDF, Markdown, or plain text.

---

## 2. Feature Breakdown

### 2.1 MVP Features (Sprint 1–3)

| Feature | Priority | Complexity |
|---|---|---|
| User registration & login (JWT) | P0 | Medium |
| Create / list / delete meetings | P0 | Low |
| Audio recording in app | P0 | High |
| Audio file upload | P0 | Medium |
| Whisper speech-to-text | P0 | Medium |
| Executive summary generation | P0 | Medium |
| Action item extraction | P0 | Medium |
| Decision extraction | P0 | Medium |
| Meeting list & detail screens | P0 | Medium |
| Basic keyword search | P0 | Low |

### 2.2 Phase 2 Features (Sprint 4–5)

| Feature | Priority | Complexity |
|---|---|---|
| Chat with meeting (RAG) | P1 | High |
| Semantic vector search | P1 | High |
| Transcript chunking & embeddings | P1 | High |
| Export PDF / Markdown / TXT | P1 | Medium |
| Refresh token rotation | P1 | Medium |
| Offline support (local SQLite cache) | P1 | High |
| Push notifications for processing | P1 | Medium |

### 2.3 Future Features (Sprint 6+)

- Speaker diarisation (who said what)
- Calendar integration (auto-create meetings from invites)
- Jira / Trello action item sync
- Team workspaces & sharing
- Meeting templates
- Analytics dashboard
- Real-time live transcription

---

## 3. System Architecture

### 3.1 Technology Stack

| Layer | Technology | Reason |
|---|---|---|
| Mobile | .NET MAUI + CommunityToolkit.MVVM | Cross-platform iOS/Android |
| API | FastAPI (Python 3.12) | Async-native, direct AI ecosystem access |
| ORM | SQLAlchemy 2.0 (async) | Full async PostgreSQL support |
| Migrations | Alembic | SQLAlchemy-native migration tool |
| Validation | Pydantic v2 | Built into FastAPI, fast, typed |
| Auth | python-jose + passlib | JWT + bcrypt |
| Background Jobs | Celery + Redis | Distributed task queue |
| Database | PostgreSQL + pgvector | Relational + vector search |
| Storage | Azure Blob Storage | Audio file storage |
| AI | OpenAI SDK (Python) | Whisper, GPT-4o, Embeddings |
| Monitoring | Azure Application Insights | Telemetry + logging |
| Docs | FastAPI auto-docs (Swagger + ReDoc) | Zero-config, auto-generated |

### 3.2 High-Level Architecture

```
┌──────────────────────────────────────────────────────────────────────┐
│                           CLIENT LAYER                               │
│   ┌──────────────────────────────────────────────────────────────┐   │
│   │              .NET MAUI Mobile App (iOS / Android)            │   │
│   │       MVVM | CommunityToolkit.MVVM | Local SQLite Cache      │   │
│   └──────────────────────────────────────────────────────────────┘   │
└──────────────────────────────┬───────────────────────────────────────┘
                               │ HTTPS / REST
┌──────────────────────────────▼───────────────────────────────────────┐
│                        API GATEWAY LAYER                             │
│            Azure API Management (Rate Limiting, Auth, Logging)       │
└──────────────────────────────┬───────────────────────────────────────┘
                               │
┌──────────────────────────────▼───────────────────────────────────────┐
│                  BACKEND LAYER (FastAPI on Azure App Service)        │
│                                                                      │
│  ┌──────────────┐  ┌──────────────┐  ┌─────────────────────────┐    │
│  │  Auth Router │  │Meeting Router│  │  Summary / Chat Router  │    │
│  │  /api/v1/auth│  │/api/v1/meet. │  │  /api/v1/summary, chat  │    │
│  └──────────────┘  └──────────────┘  └─────────────────────────┘    │
│                                                                      │
│  ┌───────────────────────────────────────────────────────────────┐   │
│  │              Celery Workers (Azure Container Apps)            │   │
│  │    transcription_task | summary_task | embedding_task         │   │
│  └───────────────────────────────────────────────────────────────┘   │
└───────────────┬────────────────┬────────────────┬────────────────────┘
                │                │                │
┌───────────────▼──┐  ┌──────────▼──────┐  ┌─────▼───────────────────┐
│  PostgreSQL DB   │  │ Azure Blob      │  │ OpenAI Python SDK       │
│  + pgvector      │  │ Storage         │  │  - Whisper (STT)        │
│  SQLAlchemy ORM  │  │ (Audio Files)   │  │  - GPT-4o (LLM)         │
│  Alembic Migrate │  │                 │  │  - text-embedding-3     │
└──────────────────┘  └─────────────────┘  └─────────────────────────┘
                                │
                      ┌─────────▼────────┐
                      │  Redis           │
                      │  (Celery broker  │
                      │   + result store)│
                      └──────────────────┘
```

### 3.3 Component Diagram

```
backend/
├── app/
│   ├── api/v1/              # FastAPI Routers (Controllers)
│   ├── services/            # Business Logic Layer
│   ├── models/              # SQLAlchemy ORM Models
│   ├── schemas/             # Pydantic Request/Response Models
│   ├── workers/             # Celery Background Tasks
│   ├── core/                # Config, Security, Dependencies
│   └── db/                  # DB Session, Base Model
└── alembic/                 # Database Migrations
```

### 3.4 Data Flow — Audio to Summary

```
User Records / Uploads Audio
        │
        ▼
POST /api/v1/meetings/{id}/audio
FastAPI uploads file → Azure Blob Storage
        │
        ▼
Publish task: transcription_task.delay(meeting_id, blob_url)
        │
        ▼
Celery Worker: transcription_task
  - Download audio from Blob
  - Split if > 25MB (ffmpeg-python)
  - Call openai.audio.transcriptions.create (Whisper)
  - Save transcript to PostgreSQL
  - Chain: summary_task.delay(meeting_id)
        │
        ▼
Celery Worker: summary_task
  - Call GPT-4o for executive summary
  - Call GPT-4o (JSON mode) for action items
  - Call GPT-4o (JSON mode) for decisions
  - Save all to PostgreSQL
  - Chain: embedding_task.delay(meeting_id)
        │
        ▼
Celery Worker: embedding_task
  - Chunk transcript (500 tokens, 50 overlap)
  - Call openai.embeddings.create (text-embedding-3-small)
  - Store vectors in transcript_chunks (pgvector)
  - Update meeting.status = 'completed'
```

---

## 4. Database Design

### 4.1 ER Diagram

```
users ──< meetings ──< transcripts ──< transcript_chunks (pgvector)
                  ──< summaries
                  ──< action_items
                  ──< decisions
                  ──< chat_messages
users ──< refresh_tokens
```

### 4.2 Tables

#### users
```sql
CREATE TABLE users (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email           VARCHAR(255) NOT NULL UNIQUE,
    password_hash   VARCHAR(512) NOT NULL,
    full_name       VARCHAR(255) NOT NULL,
    avatar_url      VARCHAR(1024),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_users_email ON users(email);
```

#### refresh_tokens
```sql
CREATE TABLE refresh_tokens (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id         UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash      VARCHAR(512) NOT NULL UNIQUE,
    expires_at      TIMESTAMPTZ NOT NULL,
    revoked         BOOLEAN NOT NULL DEFAULT FALSE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_refresh_tokens_user ON refresh_tokens(user_id);
```

#### meetings
```sql
CREATE TABLE meetings (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id         UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title           VARCHAR(512) NOT NULL,
    description     TEXT,
    meeting_date    TIMESTAMPTZ NOT NULL,
    duration_secs   INTEGER,
    audio_blob_url  VARCHAR(2048),
    audio_file_name VARCHAR(512),
    status          VARCHAR(50) NOT NULL DEFAULT 'pending',
    -- status: pending | transcribing | summarising | embedding | completed | failed
    error_message   TEXT,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_meetings_user ON meetings(user_id);
CREATE INDEX idx_meetings_date ON meetings(meeting_date DESC);
```

#### transcripts
```sql
CREATE TABLE transcripts (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    meeting_id      UUID NOT NULL REFERENCES meetings(id) ON DELETE CASCADE UNIQUE,
    raw_text        TEXT NOT NULL,
    language        VARCHAR(10) DEFAULT 'en',
    word_count      INTEGER,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

#### transcript_chunks
```sql
CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE transcript_chunks (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    transcript_id   UUID NOT NULL REFERENCES transcripts(id) ON DELETE CASCADE,
    meeting_id      UUID NOT NULL REFERENCES meetings(id) ON DELETE CASCADE,
    chunk_index     INTEGER NOT NULL,
    chunk_text      TEXT NOT NULL,
    embedding       vector(1536),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_chunks_transcript ON transcript_chunks(transcript_id);
CREATE INDEX idx_chunks_embedding ON transcript_chunks
    USING ivfflat (embedding vector_cosine_ops) WITH (lists = 100);
```

#### summaries
```sql
CREATE TABLE summaries (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    meeting_id          UUID NOT NULL REFERENCES meetings(id) ON DELETE CASCADE UNIQUE,
    executive_summary   TEXT,
    detailed_summary    TEXT,
    highlights          JSONB,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
```

#### action_items
```sql
CREATE TABLE action_items (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    meeting_id      UUID NOT NULL REFERENCES meetings(id) ON DELETE CASCADE,
    task            TEXT NOT NULL,
    owner           VARCHAR(255),
    priority        VARCHAR(20) DEFAULT 'medium',
    due_date        DATE,
    is_completed    BOOLEAN DEFAULT FALSE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_action_items_meeting ON action_items(meeting_id);
```

#### decisions
```sql
CREATE TABLE decisions (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    meeting_id      UUID NOT NULL REFERENCES meetings(id) ON DELETE CASCADE,
    decision_text   TEXT NOT NULL,
    made_by         VARCHAR(255),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_decisions_meeting ON decisions(meeting_id);
```

#### chat_messages
```sql
CREATE TABLE chat_messages (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    meeting_id      UUID NOT NULL REFERENCES meetings(id) ON DELETE CASCADE,
    user_id         UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role            VARCHAR(20) NOT NULL,
    content         TEXT NOT NULL,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX idx_chat_meeting ON chat_messages(meeting_id, created_at ASC);
```

---

## 5. Backend Design

### 5.1 Folder Structure

```
backend/
├── app/
│   ├── main.py                        # FastAPI app, lifespan, middleware
│   ├── core/
│   │   ├── config.py                  # pydantic-settings (env vars)
│   │   ├── security.py                # JWT encode/decode, password hash
│   │   └── dependencies.py            # get_db, get_current_user, get_services
│   ├── api/
│   │   └── v1/
│   │       ├── router.py              # Aggregate all routers
│   │       ├── auth.py                # /auth endpoints
│   │       ├── meetings.py            # /meetings endpoints
│   │       ├── transcripts.py         # /meetings/{id}/transcript
│   │       ├── summaries.py           # /meetings/{id}/summary
│   │       └── chat.py                # /meetings/{id}/chat
│   ├── models/                        # SQLAlchemy ORM models
│   │   ├── base.py
│   │   ├── user.py
│   │   ├── meeting.py
│   │   ├── transcript.py
│   │   ├── transcript_chunk.py
│   │   ├── summary.py
│   │   ├── action_item.py
│   │   ├── decision.py
│   │   └── chat_message.py
│   ├── schemas/                       # Pydantic v2 schemas
│   │   ├── auth.py
│   │   ├── meeting.py
│   │   ├── transcript.py
│   │   ├── summary.py
│   │   └── chat.py
│   ├── services/                      # Business logic
│   │   ├── meeting_service.py
│   │   ├── auth_service.py
│   │   ├── ai_service.py              # GPT-4o (summary, chat)
│   │   ├── whisper_service.py         # OpenAI Whisper
│   │   ├── embedding_service.py       # text-embedding-3-small
│   │   ├── vector_search_service.py   # pgvector similarity search
│   │   ├── storage_service.py         # Azure Blob Storage
│   │   └── export_service.py          # PDF/MD/TXT export
│   ├── workers/                       # Celery tasks
│   │   ├── celery_app.py
│   │   ├── transcription_task.py
│   │   ├── summary_task.py
│   │   └── embedding_task.py
│   ├── prompts/                       # Prompt templates
│   │   └── templates.py
│   └── db/
│       ├── session.py                 # Async SQLAlchemy engine + session
│       └── base.py
├── alembic/
│   ├── env.py
│   └── versions/
├── tests/
│   ├── unit/
│   ├── integration/
│   └── ai_evals/
├── pyproject.toml                     # Dependencies (uv or poetry)
├── Dockerfile
├── docker-compose.yml                 # Local dev: postgres, redis, api, worker
└── .env.example
```

### 5.2 Core Files

#### `app/main.py`
```python
from contextlib import asynccontextmanager
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware
from app.api.v1.router import api_router
from app.core.config import settings
from app.db.session import engine
from app.models.base import Base

@asynccontextmanager
async def lifespan(app: FastAPI):
    # Startup
    async with engine.begin() as conn:
        await conn.run_sync(Base.metadata.create_all)
    yield
    # Shutdown
    await engine.dispose()

app = FastAPI(
    title="MeetingNotes API",
    version="2.0.0",
    docs_url="/docs",
    redoc_url="/redoc",
    lifespan=lifespan
)

app.add_middleware(
    CORSMiddleware,
    allow_origins=settings.ALLOWED_ORIGINS,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(api_router, prefix="/api/v1")
```

#### `app/core/config.py`
```python
from pydantic_settings import BaseSettings, SettingsConfigDict

class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", extra="ignore")

    # Database
    DATABASE_URL: str
    # Redis / Celery
    REDIS_URL: str = "redis://localhost:6379/0"
    # JWT
    JWT_SECRET_KEY: str
    JWT_ALGORITHM: str = "HS256"
    ACCESS_TOKEN_EXPIRE_MINUTES: int = 15
    REFRESH_TOKEN_EXPIRE_DAYS: int = 30
    # OpenAI
    OPENAI_API_KEY: str
    OPENAI_MODEL: str = "gpt-4o"
    WHISPER_MODEL: str = "whisper-1"
    EMBEDDING_MODEL: str = "text-embedding-3-small"
    # Azure Storage
    AZURE_STORAGE_CONNECTION_STRING: str
    AZURE_BLOB_CONTAINER: str = "meeting-audio"
    # App
    ALLOWED_ORIGINS: list[str] = ["http://localhost:3000"]

settings = Settings()
```

#### `app/core/dependencies.py`
```python
from typing import AsyncGenerator
from fastapi import Depends, HTTPException, status
from fastapi.security import HTTPBearer, HTTPAuthorizationCredentials
from sqlalchemy.ext.asyncio import AsyncSession
from app.db.session import AsyncSessionLocal
from app.core.security import decode_access_token
from app.models.user import User
from app.services.auth_service import AuthService

security = HTTPBearer()

async def get_db() -> AsyncGenerator[AsyncSession, None]:
    async with AsyncSessionLocal() as session:
        try:
            yield session
            await session.commit()
        except Exception:
            await session.rollback()
            raise

async def get_current_user(
    credentials: HTTPAuthorizationCredentials = Depends(security),
    db: AsyncSession = Depends(get_db)
) -> User:
    token = credentials.credentials
    payload = decode_access_token(token)
    if not payload:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED,
                            detail="Invalid or expired token")
    user = await AuthService(db).get_user_by_id(payload["sub"])
    if not user:
        raise HTTPException(status_code=status.HTTP_401_UNAUTHORIZED,
                            detail="User not found")
    return user
```

### 5.3 Router Pattern

#### `app/api/v1/meetings.py`
```python
from uuid import UUID
from fastapi import APIRouter, Depends, HTTPException, UploadFile, File, status, BackgroundTasks
from sqlalchemy.ext.asyncio import AsyncSession
from app.core.dependencies import get_db, get_current_user
from app.models.user import User
from app.schemas.meeting import (
    CreateMeetingRequest, MeetingResponse, MeetingDetailResponse, MeetingListResponse
)
from app.services.meeting_service import MeetingService
from app.workers.transcription_task import transcribe_audio_task

router = APIRouter(prefix="/meetings", tags=["Meetings"])

@router.get("/", response_model=MeetingListResponse)
async def list_meetings(
    page: int = 1,
    page_size: int = 20,
    search: str | None = None,
    current_user: User = Depends(get_current_user),
    db: AsyncSession = Depends(get_db)
):
    service = MeetingService(db)
    return await service.list_meetings(current_user.id, page, page_size, search)

@router.post("/", response_model=MeetingResponse, status_code=201)
async def create_meeting(
    request: CreateMeetingRequest,
    current_user: User = Depends(get_current_user),
    db: AsyncSession = Depends(get_db)
):
    return await MeetingService(db).create_meeting(request, current_user.id)

@router.get("/{meeting_id}", response_model=MeetingDetailResponse)
async def get_meeting(
    meeting_id: UUID,
    current_user: User = Depends(get_current_user),
    db: AsyncSession = Depends(get_db)
):
    meeting = await MeetingService(db).get_meeting(meeting_id, current_user.id)
    if not meeting:
        raise HTTPException(status_code=404, detail="Meeting not found")
    return meeting

@router.delete("/{meeting_id}", status_code=204)
async def delete_meeting(
    meeting_id: UUID,
    current_user: User = Depends(get_current_user),
    db: AsyncSession = Depends(get_db)
):
    await MeetingService(db).delete_meeting(meeting_id, current_user.id)

@router.post("/{meeting_id}/audio", status_code=202)
async def upload_audio(
    meeting_id: UUID,
    file: UploadFile = File(...),
    current_user: User = Depends(get_current_user),
    db: AsyncSession = Depends(get_db)
):
    service = MeetingService(db)
    blob_url = await service.upload_audio(meeting_id, current_user.id, file)
    # Dispatch Celery task
    transcribe_audio_task.delay(str(meeting_id), blob_url)
    return {"message": "Audio uploaded. Transcription started.", "meeting_id": meeting_id}
```

### 5.4 Service Pattern

#### `app/services/meeting_service.py`
```python
from uuid import UUID
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select, delete
from app.models.meeting import Meeting
from app.schemas.meeting import CreateMeetingRequest
from app.services.storage_service import AzureBlobStorageService

class MeetingService:
    def __init__(self, db: AsyncSession):
        self.db = db
        self.storage = AzureBlobStorageService()

    async def create_meeting(self, request: CreateMeetingRequest, user_id: UUID) -> Meeting:
        meeting = Meeting(
            user_id=user_id,
            title=request.title,
            description=request.description,
            meeting_date=request.meeting_date,
            status="pending"
        )
        self.db.add(meeting)
        await self.db.flush()
        return meeting

    async def get_meeting(self, meeting_id: UUID, user_id: UUID) -> Meeting | None:
        result = await self.db.execute(
            select(Meeting).where(
                Meeting.id == meeting_id,
                Meeting.user_id == user_id  # Ownership check
            )
        )
        return result.scalar_one_or_none()

    async def upload_audio(self, meeting_id: UUID, user_id: UUID, file) -> str:
        meeting = await self.get_meeting(meeting_id, user_id)
        if not meeting:
            raise ValueError("Meeting not found")
        blob_url = await self.storage.upload(
            container="meeting-audio",
            blob_name=f"{meeting_id}/{file.filename}",
            data=await file.read(),
            content_type=file.content_type
        )
        meeting.audio_blob_url = blob_url
        meeting.audio_file_name = file.filename
        meeting.status = "transcribing"
        return blob_url
```

### 5.5 Celery Workers

#### `app/workers/celery_app.py`
```python
from celery import Celery
from app.core.config import settings

celery = Celery(
    "meeting_notes",
    broker=settings.REDIS_URL,
    backend=settings.REDIS_URL,
    include=[
        "app.workers.transcription_task",
        "app.workers.summary_task",
        "app.workers.embedding_task",
    ]
)
celery.conf.task_serializer = "json"
celery.conf.result_serializer = "json"
celery.conf.timezone = "UTC"
```

#### `app/workers/transcription_task.py`
```python
from app.workers.celery_app import celery
from app.workers.summary_task import generate_summary_task

@celery.task(bind=True, max_retries=3, default_retry_delay=60)
def transcribe_audio_task(self, meeting_id: str, blob_url: str):
    try:
        from app.services.whisper_service import WhisperService
        from app.services.storage_service import AzureBlobStorageService
        from app.db.session import SyncSessionLocal
        from app.models.meeting import Meeting
        from app.models.transcript import Transcript

        storage = AzureBlobStorageService()
        audio_bytes = storage.download_sync(blob_url)

        whisper = WhisperService()
        transcript_text = whisper.transcribe(audio_bytes)

        with SyncSessionLocal() as db:
            # Save transcript
            transcript = Transcript(
                meeting_id=meeting_id,
                raw_text=transcript_text,
                word_count=len(transcript_text.split())
            )
            db.add(transcript)
            # Update meeting status
            meeting = db.get(Meeting, meeting_id)
            meeting.status = "summarising"
            db.commit()

        # Chain to next task
        generate_summary_task.delay(meeting_id, transcript_text)

    except Exception as exc:
        raise self.retry(exc=exc)
```

---

## 6. API Design

### 6.1 REST Endpoints

#### Auth — `/api/v1/auth`

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/auth/register` | Register new user |
| `POST` | `/auth/login` | Login → JWT + refresh token |
| `POST` | `/auth/refresh` | Exchange refresh token for new pair |
| `POST` | `/auth/logout` | Revoke refresh token |
| `POST` | `/auth/forgot-password` | Send reset email |
| `POST` | `/auth/reset-password` | Reset with token |
| `GET` | `/auth/profile` | Get current user |
| `PUT` | `/auth/profile` | Update profile |

#### Meetings — `/api/v1/meetings`

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/meetings` | List meetings (paginated, search) |
| `POST` | `/meetings` | Create meeting |
| `GET` | `/meetings/{id}` | Get meeting detail |
| `PUT` | `/meetings/{id}` | Update meeting |
| `DELETE` | `/meetings/{id}` | Delete meeting |
| `POST` | `/meetings/{id}/audio` | Upload audio file (multipart) |
| `GET` | `/meetings/{id}/status` | Poll processing status |

#### Content — `/api/v1/meetings/{id}`

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/{id}/transcript` | Get full transcript |
| `GET` | `/{id}/summary` | Get summary + action items + decisions |
| `POST` | `/{id}/chat` | Send message, get RAG answer |
| `GET` | `/{id}/chat` | Get chat history |
| `GET` | `/{id}/export` | Export (`?format=pdf\|md\|txt`) |

#### Search — `/api/v1/search`

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/search?q={query}&mode=semantic` | Full-text or semantic search |

### 6.2 Pydantic Schemas

#### `schemas/meeting.py`
```python
from pydantic import BaseModel, Field
from datetime import datetime
from uuid import UUID
from enum import Enum

class MeetingStatus(str, Enum):
    pending = "pending"
    transcribing = "transcribing"
    summarising = "summarising"
    embedding = "embedding"
    completed = "completed"
    failed = "failed"

class CreateMeetingRequest(BaseModel):
    title: str = Field(..., min_length=1, max_length=512)
    description: str | None = None
    meeting_date: datetime

class MeetingResponse(BaseModel):
    id: UUID
    title: str
    status: MeetingStatus
    meeting_date: datetime
    duration_secs: int | None
    created_at: datetime
    model_config = {"from_attributes": True}

class ActionItemSchema(BaseModel):
    id: UUID
    task: str
    owner: str | None
    priority: str
    due_date: str | None
    is_completed: bool
    model_config = {"from_attributes": True}

class DecisionSchema(BaseModel):
    id: UUID
    decision_text: str
    made_by: str | None
    model_config = {"from_attributes": True}

class SummaryResponse(BaseModel):
    meeting_id: UUID
    executive_summary: str | None
    detailed_summary: str | None
    highlights: list[str]
    action_items: list[ActionItemSchema]
    decisions: list[DecisionSchema]
    model_config = {"from_attributes": True}
```

#### `schemas/chat.py`
```python
from pydantic import BaseModel
from uuid import UUID

class ChatMessageSchema(BaseModel):
    role: str   # "user" | "assistant"
    content: str

class ChatRequest(BaseModel):
    question: str
    history: list[ChatMessageSchema] = []

class SourceChunk(BaseModel):
    chunk_index: int
    relevance_score: float
    preview: str

class ChatResponse(BaseModel):
    answer: str
    source_chunks: list[SourceChunk] = []
```

### 6.3 Example Request/Response

#### POST /api/v1/meetings/{id}/chat
```json
// Request
{
  "question": "What tasks were assigned to Arjun?",
  "history": [
    { "role": "user", "content": "What was the launch date discussed?" },
    { "role": "assistant", "content": "The launch date confirmed was November 15th." }
  ]
}

// Response 200
{
  "answer": "Arjun was assigned one task: preparing the technical specification for Feature X, due March 22nd, marked as high priority.",
  "source_chunks": [
    {
      "chunk_index": 4,
      "relevance_score": 0.94,
      "preview": "...Arjun will handle the tech spec by end of next week..."
    }
  ]
}
```

#### GET /api/v1/meetings/{id}/summary
```json
{
  "meeting_id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "executive_summary": "The team aligned on a Q4 roadmap prioritising three features...",
  "detailed_summary": "The meeting opened with a review of Q3 metrics...",
  "highlights": [
    "Launch date confirmed as November 15th",
    "Budget approved for infrastructure upgrade",
    "New hire approved for mobile team"
  ],
  "action_items": [
    {
      "id": "...",
      "task": "Prepare technical spec for Feature X",
      "owner": "Arjun",
      "priority": "high",
      "due_date": "2025-03-22",
      "is_completed": false
    }
  ],
  "decisions": [
    {
      "id": "...",
      "decision_text": "Approved use of Azure for all new infrastructure",
      "made_by": "Sarah"
    }
  ]
}
```

---

## 7. AI Design

### 7.1 Prompt Templates (`app/prompts/templates.py`)

#### Executive Summary
```python
EXECUTIVE_SUMMARY_SYSTEM = """
You are an expert meeting analyst. Produce a concise, professional executive summary.

Rules:
- Maximum 150 words
- Use past tense
- Be objective and neutral
- Focus on outcomes, not process
- Do not use filler like "The meeting discussed..."
"""

EXECUTIVE_SUMMARY_USER = """
Transcript:
{transcript}

Generate an executive summary.
"""
```

#### Action Item Extraction (JSON mode)
```python
ACTION_ITEMS_SYSTEM = """
You are an expert at extracting structured action items from meeting transcripts.

Return ONLY a valid JSON array. No markdown, no explanation, no preamble.

Schema:
[{
  "task": "string (clear, actionable description)",
  "owner": "string | null",
  "priority": "low | medium | high",
  "due_date": "YYYY-MM-DD | null"
}]

Rules:
- Only include concrete tasks, not vague intentions
- Infer priority from urgency language (ASAP = high, eventually = low)
- Infer due date ONLY if explicitly stated
"""
```

#### Decision Extraction (JSON mode)
```python
DECISIONS_SYSTEM = """
You are an expert at identifying key decisions from meeting transcripts.
A decision is a clear resolution or agreement reached by the group.

Return ONLY a valid JSON array.

Schema:
[{
  "decision_text": "string (clear statement of what was decided)",
  "made_by": "string | null"
}]
"""
```

#### RAG Chat System Prompt
```python
RAG_CHAT_SYSTEM = """
You are an intelligent meeting assistant. Answer questions using ONLY the provided context.

Rules:
- Answer ONLY from the context below
- If not found, say: "I couldn't find that in this meeting's transcript"
- Be concise (1-3 sentences unless asked for detail)
- Reference specific names or items when relevant
- Never fabricate information

Context from meeting transcript:
{context}
"""
```

### 7.2 AI Service (`app/services/ai_service.py`)

```python
import json
from openai import AsyncOpenAI
from app.core.config import settings
from app.prompts.templates import (
    EXECUTIVE_SUMMARY_SYSTEM, EXECUTIVE_SUMMARY_USER,
    ACTION_ITEMS_SYSTEM, DECISIONS_SYSTEM, RAG_CHAT_SYSTEM
)

class AIService:
    def __init__(self):
        self.client = AsyncOpenAI(api_key=settings.OPENAI_API_KEY)
        self.model = settings.OPENAI_MODEL

    async def generate_executive_summary(self, transcript: str) -> str:
        response = await self.client.chat.completions.create(
            model=self.model,
            messages=[
                {"role": "system", "content": EXECUTIVE_SUMMARY_SYSTEM},
                {"role": "user", "content": EXECUTIVE_SUMMARY_USER.format(transcript=transcript)}
            ],
            max_tokens=300,
            temperature=0.3
        )
        return response.choices[0].message.content

    async def extract_action_items(self, transcript: str) -> list[dict]:
        response = await self.client.chat.completions.create(
            model=self.model,
            response_format={"type": "json_object"},
            messages=[
                {"role": "system", "content": ACTION_ITEMS_SYSTEM},
                {"role": "user", "content": f"Transcript:\n{transcript}"}
            ],
            max_tokens=1000,
            temperature=0.1
        )
        return json.loads(response.choices[0].message.content)

    async def extract_decisions(self, transcript: str) -> list[dict]:
        response = await self.client.chat.completions.create(
            model=self.model,
            response_format={"type": "json_object"},
            messages=[
                {"role": "system", "content": DECISIONS_SYSTEM},
                {"role": "user", "content": f"Transcript:\n{transcript}"}
            ],
            max_tokens=500,
            temperature=0.1
        )
        return json.loads(response.choices[0].message.content)

    async def chat_with_meeting(
        self, context: str, question: str, history: list[dict]
    ) -> str:
        messages = [
            {"role": "system", "content": RAG_CHAT_SYSTEM.format(context=context)},
            *history[-6:],
            {"role": "user", "content": question}
        ]
        response = await self.client.chat.completions.create(
            model=self.model,
            messages=messages,
            max_tokens=512,
            temperature=0.2
        )
        return response.choices[0].message.content

    async def get_embedding(self, text: str) -> list[float]:
        response = await self.client.embeddings.create(
            model=settings.EMBEDDING_MODEL,
            input=text
        )
        return response.data[0].embedding
```

### 7.3 Embedding Pipeline (`app/services/embedding_service.py`)

```python
import tiktoken
from app.services.ai_service import AIService

class EmbeddingService:
    CHUNK_SIZE = 500     # tokens
    CHUNK_OVERLAP = 50   # tokens

    def __init__(self):
        self.ai = AIService()
        self.tokenizer = tiktoken.get_encoding("cl100k_base")

    def chunk_text(self, text: str) -> list[str]:
        tokens = self.tokenizer.encode(text)
        chunks = []
        start = 0
        while start < len(tokens):
            end = min(start + self.CHUNK_SIZE, len(tokens))
            chunk_tokens = tokens[start:end]
            chunks.append(self.tokenizer.decode(chunk_tokens))
            start += self.CHUNK_SIZE - self.CHUNK_OVERLAP
        return chunks

    async def embed_transcript(self, transcript_id: str, meeting_id: str, text: str):
        chunks = self.chunk_text(text)
        results = []
        for i, chunk in enumerate(chunks):
            embedding = await self.ai.get_embedding(chunk)
            results.append({
                "transcript_id": transcript_id,
                "meeting_id": meeting_id,
                "chunk_index": i,
                "chunk_text": chunk,
                "embedding": embedding
            })
        return results
```

### 7.4 Vector Search (`app/services/vector_search_service.py`)

```python
from uuid import UUID
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import text

class VectorSearchService:
    def __init__(self, db: AsyncSession):
        self.db = db

    async def search(
        self, meeting_id: UUID, query_embedding: list[float], top_k: int = 5
    ) -> list[dict]:
        embedding_str = "[" + ",".join(str(x) for x in query_embedding) + "]"
        result = await self.db.execute(
            text("""
                SELECT chunk_index, chunk_text,
                       1 - (embedding <=> :embedding::vector) AS score
                FROM transcript_chunks
                WHERE meeting_id = :meeting_id
                ORDER BY embedding <=> :embedding::vector
                LIMIT :top_k
            """),
            {
                "embedding": embedding_str,
                "meeting_id": str(meeting_id),
                "top_k": top_k
            }
        )
        rows = result.fetchall()
        return [
            {"chunk_index": r[0], "chunk_text": r[1], "score": float(r[2])}
            for r in rows
        ]
```

### 7.5 RAG Chat Flow

```
User Question (string)
        │
        ▼
1. ai_service.get_embedding(question)          → vector[1536]
        │
        ▼
2. vector_search.search(meeting_id, vector)    → top 5 chunks
        │
        ▼
3. Build context = "\n\n".join(chunk["chunk_text"] for chunk in chunks)
        │
        ▼
4. ai_service.chat_with_meeting(context, question, history)
        │
        ▼
5. Persist user + assistant messages to chat_messages table
        │
        ▼
6. Return ChatResponse { answer, source_chunks }
```

---

## 8. Mobile Design

### 8.1 Screen List

| Screen | Description |
|---|---|
| `SplashPage` | App launch, check JWT in SecureStorage |
| `LoginPage` | Email/password login |
| `RegisterPage` | New account creation |
| `ForgotPasswordPage` | Send password reset |
| `MeetingsListPage` | All meetings with search bar |
| `MeetingDetailPage` | Tabs: Summary / Transcript / Chat |
| `RecordMeetingPage` | Live recording with timer |
| `UploadAudioPage` | File picker |
| `SummaryPage` | Summary, action items, decisions |
| `TranscriptPage` | Full scrollable transcript |
| `ChatPage` | Conversational chat with meeting |
| `ExportPage` | Choose format and share |
| `ProfilePage` | User profile settings |

### 8.2 Navigation Flow

```
App Start
   │
   ├── No token ──► LoginPage ──► RegisterPage / ForgotPasswordPage
   │
   └── Token valid ──► Shell (AppShell.xaml)
                           │
                           └── MeetingsListPage  [Root]
                                   │
                                   ├── [FAB +] ──► CreateMeetingPage
                                   │                  ├── RecordMeetingPage
                                   │                  └── UploadAudioPage
                                   │
                                   └── [Row tap] ──► MeetingDetailPage
                                                         ├── Tab: Summary
                                                         ├── Tab: Transcript
                                                         ├── Tab: Chat
                                                         └── [⋮ Export]
```

### 8.3 MVVM Structure

#### `MeetingsListViewModel.cs`
```csharp
public partial class MeetingsListViewModel : BaseViewModel
{
    private readonly IMeetingService _meetingService;

    [ObservableProperty] private ObservableCollection<MeetingDto> _meetings = new();
    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private bool _isRefreshing;

    partial void OnSearchQueryChanged(string value) => SearchCommand.Execute(null);

    [RelayCommand]
    private async Task LoadMeetingsAsync()
    {
        IsRefreshing = true;
        var result = await _meetingService.GetMeetingsAsync();
        Meetings = new ObservableCollection<MeetingDto>(result);
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        var result = await _meetingService.GetMeetingsAsync(search: SearchQuery);
        Meetings = new ObservableCollection<MeetingDto>(result);
    }

    [RelayCommand]
    private async Task NavigateToMeetingAsync(MeetingDto meeting)
        => await Shell.Current.GoToAsync($"{nameof(MeetingDetailPage)}?id={meeting.Id}");
}
```

#### `ChatViewModel.cs`
```csharp
public partial class ChatViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IMeetingService _service;
    private Guid _meetingId;

    [ObservableProperty] private ObservableCollection<ChatMessageDto> _messages = new();
    [ObservableProperty] private string _inputText = string.Empty;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
        => _meetingId = Guid.Parse(query["id"].ToString()!);

    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(InputText)) return;
        var question = InputText;
        InputText = string.Empty;

        Messages.Add(new ChatMessageDto { Role = "user", Content = question });

        var response = await _service.SendChatMessageAsync(_meetingId, new ChatRequest
        {
            Question = question,
            History = Messages.TakeLast(6).Select(m => new ChatHistoryItem
                { Role = m.Role, Content = m.Content }).ToList()
        });

        Messages.Add(new ChatMessageDto { Role = "assistant", Content = response.Answer });
    }
}
```

### 8.4 Mobile Services Interface

```csharp
public interface IMeetingService
{
    Task<List<MeetingDto>> GetMeetingsAsync(string? search = null, int page = 1);
    Task<MeetingDetailDto> GetMeetingAsync(Guid id);
    Task<Guid> CreateMeetingAsync(CreateMeetingRequest request);
    Task UploadAudioAsync(Guid meetingId, Stream stream, string fileName);
    Task<MeetingStatusDto> GetStatusAsync(Guid meetingId);
    Task<SummaryDto> GetSummaryAsync(Guid meetingId);
    Task<string> GetTranscriptAsync(Guid meetingId);
    Task<ChatResponseDto> SendChatMessageAsync(Guid meetingId, ChatRequest request);
    Task<byte[]> ExportMeetingAsync(Guid meetingId, ExportFormat format);
}

public interface IAudioRecordingService
{
    Task StartAsync(string outputPath);
    Task PauseAsync();
    Task ResumeAsync();
    Task<string> StopAsync();  // Returns local file path
    TimeSpan Elapsed { get; }
}
```

---

## 9. Security Design

### 9.1 JWT + Refresh Token

#### `app/core/security.py`
```python
from datetime import datetime, timedelta, timezone
from jose import JWTError, jwt
from passlib.context import CryptContext
from app.core.config import settings

pwd_context = CryptContext(schemes=["bcrypt"], deprecated="auto")

def hash_password(password: str) -> str:
    return pwd_context.hash(password)

def verify_password(plain: str, hashed: str) -> bool:
    return pwd_context.verify(plain, hashed)

def create_access_token(user_id: str) -> str:
    expire = datetime.now(timezone.utc) + timedelta(
        minutes=settings.ACCESS_TOKEN_EXPIRE_MINUTES
    )
    return jwt.encode(
        {"sub": user_id, "exp": expire, "type": "access"},
        settings.JWT_SECRET_KEY,
        algorithm=settings.JWT_ALGORITHM
    )

def decode_access_token(token: str) -> dict | None:
    try:
        payload = jwt.decode(token, settings.JWT_SECRET_KEY,
                             algorithms=[settings.JWT_ALGORITHM])
        if payload.get("type") != "access":
            return None
        return payload
    except JWTError:
        return None
```

### 9.2 Refresh Token Rotation

```
Client                                     FastAPI
  │                                           │
  │──── POST /auth/login ───────────────────►│
  │◄─── { access_token, refresh_token }       │  Store SHA-256(refresh) in DB
  │                                           │
  │  [15 min — access_token expires]          │
  │                                           │
  │──── POST /auth/refresh ─────────────────►│
  │     { refresh_token }                     │  Verify → revoke old → issue new pair
  │◄─── { access_token, refresh_token }       │
  │                                           │
  │  [Stolen token reuse detected]            │
  │──── POST /auth/refresh ─────────────────►│
  │     { old refresh_token }                 │  Token already revoked → revoke ALL
  │◄─── 401 Unauthorized                      │  user's tokens (security breach alert)
```

### 9.3 Security Checklist

- All routes protected with `Depends(get_current_user)` except auth endpoints
- Ownership enforced in every service method (`WHERE user_id = current_user.id`)
- Passwords hashed with bcrypt (passlib, work factor 12)
- Refresh tokens stored as SHA-256 hashes, never plaintext
- OpenAI API key loaded from Azure Key Vault via environment
- Audio blobs in private container, accessed via short-lived SAS URLs
- HTTPS enforced at Azure API Management layer
- Rate limiting: 5 req/min on auth endpoints
- Pydantic v2 validates all inputs before reaching service layer
- CORS configured to allowed origins only

### 9.4 MAUI Secure Storage

```csharp
// Tokens stored in platform secure storage
// iOS: Keychain | Android: Encrypted SharedPreferences (Keystore)

await SecureStorage.SetAsync("access_token", accessToken);
await SecureStorage.SetAsync("refresh_token", refreshToken);

// HTTP interceptor auto-refreshes expired access tokens
public class AuthHttpHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = await SecureStorage.GetAsync("access_token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await base.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Attempt refresh
            var newToken = await _authService.RefreshTokenAsync();
            if (newToken != null)
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", newToken);
                response = await base.SendAsync(request, ct);
            }
        }
        return response;
    }
}
```

---

## 10. Deployment Architecture

### 10.1 Azure Resources

| Resource | Purpose | SKU (Start) |
|---|---|---|
| Azure App Service (Linux) | Host FastAPI (Uvicorn + Gunicorn) | B2 |
| Azure Container Apps | Celery workers (auto-scale) | Consumption |
| Azure Database for PostgreSQL | Primary DB + pgvector extension | Burstable B2ms |
| Azure Blob Storage | Audio file storage | LRS Standard |
| Azure Cache for Redis | Celery broker + result backend | C1 Standard |
| Azure Service Bus | Optional: durable event queue | Standard |
| Azure API Management | Rate limiting, auth gateway | Consumption |
| Azure Key Vault | Secrets (API keys, DB creds) | Standard |
| Azure Container Registry | Docker images | Basic |
| Azure Application Insights | Monitoring + telemetry | Pay-per-use |

### 10.2 Dockerfile

```dockerfile
FROM python:3.12-slim

WORKDIR /app

# Install system deps (ffmpeg for audio splitting)
RUN apt-get update && apt-get install -y ffmpeg && rm -rf /var/lib/apt/lists/*

COPY pyproject.toml .
RUN pip install uv && uv sync --no-dev

COPY . .

EXPOSE 8000

CMD ["gunicorn", "app.main:app", "-w", "4", "-k", "uvicorn.workers.UvicornWorker",
     "--bind", "0.0.0.0:8000", "--timeout", "120"]
```

### 10.3 docker-compose.yml (Local Dev)

```yaml
version: "3.9"
services:
  api:
    build: .
    ports: ["8000:8000"]
    environment:
      DATABASE_URL: postgresql+asyncpg://postgres:password@db:5432/meetingnotes
      REDIS_URL: redis://redis:6379/0
    env_file: .env
    depends_on: [db, redis]
    volumes: [".:/app"]
    command: uvicorn app.main:app --host 0.0.0.0 --port 8000 --reload

  worker:
    build: .
    command: celery -A app.workers.celery_app worker --loglevel=info
    env_file: .env
    depends_on: [db, redis]

  db:
    image: pgvector/pgvector:pg16
    environment:
      POSTGRES_DB: meetingnotes
      POSTGRES_PASSWORD: password
    ports: ["5432:5432"]
    volumes: [pgdata:/var/lib/postgresql/data]

  redis:
    image: redis:7-alpine
    ports: ["6379:6379"]

volumes:
  pgdata:
```

### 10.4 GitHub Actions CI/CD

```yaml
# .github/workflows/deploy.yml
name: Deploy to Azure

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

jobs:
  test:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: pgvector/pgvector:pg16
        env:
          POSTGRES_PASSWORD: test
          POSTGRES_DB: test_db
        ports: ["5432:5432"]
      redis:
        image: redis:7-alpine
        ports: ["6379:6379"]

    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with:
          python-version: "3.12"
      - run: pip install uv && uv sync
      - run: uv run pytest tests/ --cov=app --cov-report=xml
      - uses: codecov/codecov-action@v4

  deploy:
    needs: test
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: azure/login@v2
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}
      - name: Build and push Docker image
        run: |
          docker build -t ${{ secrets.ACR_SERVER }}/meeting-notes:${{ github.sha }} .
          docker login ${{ secrets.ACR_SERVER }} \
            -u ${{ secrets.ACR_USERNAME }} -p ${{ secrets.ACR_PASSWORD }}
          docker push ${{ secrets.ACR_SERVER }}/meeting-notes:${{ github.sha }}
      - name: Deploy to App Service
        uses: azure/webapps-deploy@v3
        with:
          app-name: meeting-notes-api
          images: ${{ secrets.ACR_SERVER }}/meeting-notes:${{ github.sha }}
```

---

## 11. Testing Strategy

### 11.1 Unit Tests (pytest)

```python
# tests/unit/test_meeting_service.py
import pytest
from unittest.mock import AsyncMock, MagicMock
from app.services.meeting_service import MeetingService
from app.schemas.meeting import CreateMeetingRequest
from datetime import datetime

@pytest.mark.asyncio
async def test_create_meeting_success():
    mock_db = AsyncMock()
    service = MeetingService(mock_db)
    request = CreateMeetingRequest(
        title="Sprint Planning",
        meeting_date=datetime.utcnow()
    )
    user_id = "test-user-id"
    meeting = await service.create_meeting(request, user_id)
    assert meeting.title == "Sprint Planning"
    assert meeting.status == "pending"
    mock_db.add.assert_called_once()
    mock_db.flush.assert_awaited_once()
```

### 11.2 Integration Tests (pytest + Testcontainers)

```python
# tests/integration/test_meetings_api.py
import pytest
from httpx import AsyncClient
from app.main import app

@pytest.mark.asyncio
async def test_create_meeting_returns_201(auth_headers):
    async with AsyncClient(app=app, base_url="http://test") as client:
        response = await client.post(
            "/api/v1/meetings",
            json={"title": "Test Meeting", "meeting_date": "2025-03-15T10:00:00Z"},
            headers=auth_headers
        )
    assert response.status_code == 201
    assert response.json()["title"] == "Test Meeting"

@pytest.mark.asyncio
async def test_get_meeting_other_user_returns_404(auth_headers, other_user_meeting_id):
    async with AsyncClient(app=app, base_url="http://test") as client:
        response = await client.get(
            f"/api/v1/meetings/{other_user_meeting_id}",
            headers=auth_headers
        )
    assert response.status_code == 404
```

### 11.3 AI Evaluation Tests

```python
# tests/ai_evals/test_summary_quality.py
import pytest
from app.services.ai_service import AIService

SAMPLE_TRANSCRIPT = """
Arjun: We need to launch the feature by November 15th. That's confirmed.
Sarah: Agreed. I'm approving the Azure budget too.
Arjun: I'll prepare the tech spec. Should be done by March 22nd.
Priya: High priority, right? Arjun: Yes, definitely high priority.
"""

@pytest.mark.asyncio
async def test_executive_summary_mentions_launch_date():
    ai = AIService()
    summary = await ai.generate_executive_summary(SAMPLE_TRANSCRIPT)
    assert "november" in summary.lower() or "15" in summary

@pytest.mark.asyncio
async def test_action_items_extracts_arjun_task():
    ai = AIService()
    items = await ai.extract_action_items(SAMPLE_TRANSCRIPT)
    assert isinstance(items, list)
    assert any("arjun" in (i.get("owner") or "").lower() for i in items)
    assert any(i.get("priority") == "high" for i in items)

@pytest.mark.asyncio
async def test_decisions_extracts_azure_approval():
    ai = AIService()
    decisions = await ai.extract_decisions(SAMPLE_TRANSCRIPT)
    assert isinstance(decisions, list)
    assert any("azure" in d.get("decision_text", "").lower() for d in decisions)
```

### 11.4 Test Configuration (`pyproject.toml`)

```toml
[tool.pytest.ini_options]
asyncio_mode = "auto"
testpaths = ["tests"]
env = [
    "DATABASE_URL=postgresql+asyncpg://postgres:test@localhost:5432/test_db",
    "REDIS_URL=redis://localhost:6379/1",
    "JWT_SECRET_KEY=test-secret-key-for-testing-only",
]

[tool.coverage.run]
source = ["app"]
omit = ["app/workers/*", "alembic/*"]
```

---

## 12. Development Roadmap

### Sprint 1 — Foundation (Weeks 1–2)
**Goal:** Running FastAPI with auth, PostgreSQL, and basic meeting CRUD

| Task | Effort |
|---|---|
| Project setup (uv, FastAPI, SQLAlchemy, Alembic) | 0.5 day |
| PostgreSQL schema + Alembic migrations | 1 day |
| Auth endpoints (register, login, refresh, logout) | 2 days |
| JWT + bcrypt security module | 1 day |
| Meeting CRUD router + service | 2 days |
| Azure Blob Storage service | 1 day |
| Audio upload endpoint | 0.5 day |
| docker-compose local dev setup | 0.5 day |
| GitHub Actions CI pipeline | 0.5 day |
| Azure App Service deployment | 0.5 day |

**Deliverable:** Deployed FastAPI with auth and meeting management. Swagger UI live at `/docs`.

---

### Sprint 2 — AI Core (Weeks 3–4)
**Goal:** Whisper transcription + GPT-4o summary pipeline end-to-end

| Task | Effort |
|---|---|
| Celery + Redis setup (celery_app.py) | 1 day |
| WhisperService (with audio splitting for >25MB) | 2 days |
| TranscriptionWorker Celery task | 1 day |
| AIService: executive summary + detailed summary | 1.5 days |
| AIService: action item extraction (JSON mode) | 1 day |
| AIService: decision extraction (JSON mode) | 0.5 day |
| SummaryWorker Celery task | 1 day |
| AI evaluation test suite (pytest, golden dataset) | 1 day |
| Summary + transcript API endpoints | 1 day |

**Deliverable:** Upload audio → Celery processes → transcript + summary + action items in DB.

---

### Sprint 3 — Mobile MVP (Weeks 5–6)
**Goal:** Working MAUI app covering all MVP features

| Task | Effort |
|---|---|
| .NET MAUI project setup (Shell, MVVM, DI, HttpClient) | 1 day |
| AuthHttpHandler (JWT auto-refresh interceptor) | 1 day |
| Auth screens (Login, Register, Forgot Password) | 2 days |
| Meetings list screen + API integration | 1 day |
| Audio recording (platform permissions + timer UI) | 2 days |
| Audio upload + status polling loop | 1 day |
| Meeting detail page (Summary / Action Items / Decisions tabs) | 2 days |

**Deliverable:** End-to-end flow: record → upload → poll → view AI summary on mobile.

---

### Sprint 4 — RAG & Semantic Search (Weeks 7–8)
**Goal:** Chat with meeting + semantic search

| Task | Effort |
|---|---|
| pgvector: transcript_chunks table + ivfflat index | 0.5 day |
| EmbeddingService: tiktoken chunking + embedding calls | 1 day |
| EmbeddingWorker Celery task | 1 day |
| VectorSearchService: cosine similarity query | 1 day |
| Chat router + RAG chat service (full flow) | 2 days |
| Chat persistence (chat_messages table) | 0.5 day |
| Chat page in MAUI (conversation UI) | 2 days |
| Semantic search endpoint + search page in MAUI | 1 day |

**Deliverable:** Users can ask "What did we decide about pricing?" and get grounded answers.

---

### Sprint 5 — Export, Offline, Polish (Weeks 9–10)
**Goal:** Export functionality, offline support, UX polish

| Task | Effort |
|---|---|
| PDF export (ReportLab or WeasyPrint) | 2 days |
| Markdown + plain text export | 0.5 day |
| Export endpoint + MAUI export page | 1 day |
| SQLite offline cache in MAUI | 2 days |
| Background sync (online/offline reconciliation) | 1 day |
| Push notifications (Azure Notification Hubs) | 1 day |
| Error handling, retry UI, empty states | 1 day |
| UX polish (loading skeletons, animations) | 1.5 days |

**Deliverable:** Production-quality app with offline support and PDF/MD export.

---

### Sprint 6 — Hardening & Launch (Weeks 11–12)
**Goal:** Security, performance, monitoring, portfolio-ready

| Task | Effort |
|---|---|
| Application Insights integration (FastAPI middleware) | 1 day |
| Azure API Management rate limiting | 0.5 day |
| Security review (OWASP, input sanitisation audit) | 1 day |
| Load testing (Locust: transcription + chat endpoints) | 1 day |
| Integration test coverage to 80% | 2 days |
| MAUI UI test suite (Appium) | 1 day |
| Swagger docs polish (descriptions, examples) | 0.5 day |
| README, architecture diagrams, setup guide | 1 day |
| Azure cost optimisation review | 0.5 day |
| App store assets + screenshots | 0.5 day |

**Deliverable:** Production-ready portfolio project on Azure, app submitted to stores.

---

## 13. Copilot Integration Guide

### Why `.md` Files Are the Foundation

This SRS `.md` file is your **Copilot context anchor**. When open in VS Code:
- Copilot reads it as reference when generating code in adjacent files
- Copilot Chat can reference it with `#file:SRS.md` explicitly
- It's version-controlled alongside code — the spec and implementation stay in sync
- GitHub renders it beautifully in the repo browser for portfolio presentation

### Recommended Repo Structure

```
/
├── docs/
│   └── SRS.md                    ← this file (always open in VS Code)
├── backend/                      ← FastAPI project
├── mobile/                       ← .NET MAUI project
└── .github/workflows/
```

### Copilot Workflow Per Sprint

**Step 1:** Open `SRS.md` + target file side-by-side in VS Code

**Step 2:** Add a comment at the top of the new file referencing the SRS section:
```python
# See SRS Section 5.4 — MeetingService
# Implements: create_meeting, get_meeting, upload_audio, list_meetings
```

**Step 3:** Let Copilot complete from the comment. Refine as needed.

**Step 4:** Use Copilot Chat (`Ctrl+I`) for complex tasks:

| Goal | Copilot Chat Prompt |
|---|---|
| Generate a service | `Based on SRS Section 5.4, create MeetingService with async SQLAlchemy` |
| Generate a router | `Create the meetings FastAPI router matching all endpoints in SRS Section 6.1` |
| Generate Pydantic schemas | `Create Pydantic v2 schemas for meeting request/response from SRS Section 6.2` |
| Generate Celery task | `Implement the transcription_task from SRS Section 5.5 with retry logic` |
| Generate tests | `Write pytest async tests for MeetingService covering success, 404, and ownership` |
| Generate MAUI ViewModel | `Create MeetingsListViewModel using CommunityToolkit.MVVM from SRS Section 8.3` |
| Generate AI prompts | `Implement action item extraction from SRS Section 7.1 using GPT-4o JSON mode` |

### Copilot Chat with Full Codebase Context

Use `#codebase` in Copilot Chat to let it scan all open files:
```
#codebase Implement the ChatWithMeeting endpoint. 
It should use VectorSearchService and AIService 
following the RAG flow in SRS Section 7.5.
```

### Keeping the SRS in Sync

As you build, update the SRS:
- Mark completed features with ✅
- Add discovered edge cases as new requirements
- Copilot will suggest updates if you add a `TODO:` comment in the `.md` file

---

*End of Document — AI Meeting Notes Application SRS v2.0.0 (FastAPI Edition)*
