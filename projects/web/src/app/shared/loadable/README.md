# Loadable Pattern

A cohesive pattern for handling asynchronous data loading with loading and error states in Angular.

## Overview

The loadable pattern provides:

- **Type-safe wrapper** (`Loadable<T>`) for async data with loading/loaded/error states
- **Structural directive** (`*mbcLoadable`) for declarative template handling
- **Observable utilities** for converting streams to loadable format
- **Consistent UI** for loading spinners and error messages

## Quick Start

### Basic Usage

```typescript
import { LoadableDirective, toLoadable } from '@app/shared/loadable';

@Component({
  imports: [LoadableDirective],
  template: `
    <mat-card
      *mbcLoadable="state.pilot$ as pilot; errorMessage: 'Failed to load pilot'"
    >
      <h1>{{ pilot.fullName }}</h1>
      <p>{{ pilot.bio }}</p>
    </mat-card>
  `,
})
export class PilotDetailComponent {
  pilot$ = toLoadable(this.apiClient.getPilotById(id));
}
```

The directive automatically:

- Shows `<mbc-loading>` spinner while loading
- Shows `<mbc-error-state>` if an error occurs
- Renders your template with unwrapped data when loaded

### Custom Error Messages

```html
<div
  *mbcLoadable="state.data$ as data; 
                errorMessage: 'Custom error title';
                errorSubtitle: 'Detailed error message'"
>
  {{ data.value }}
</div>
```

## API Reference

### Types

```typescript
type Loadable<T> = Loading | Loaded<T> | Error;
type Loading = { status: 'loading' };
type Loaded<T> = { status: 'loaded'; value: T };
type Error = { status: 'error'; error: any };
```

### Functions

- `toLoadable<T>(source$: Observable<T>): Observable<Loadable<T>>` - Convert any observable to loadable format
- `loadableWithId<T>(id$, apiCall)` - Create loadable from ID observable
- `loadableWithParams<T>(params$, apiCall)` - Create loadable from params observable

### Directive

- **Selector:** `*mbcLoadable`
- **Inputs:**
  - `mbcLoadable` - The `Observable<Loadable<T>>` to render
  - `errorMessage` - Custom error title (optional)
  - `errorSubtitle` - Custom error message (optional)

## Advanced Patterns

### With Pagination

```typescript
@Component({
  template: `
    <div *mbcLoadable="state.items$ as items; errorMessage: 'Failed to load'">
      <table [dataSource]="items.items"></table>
      <pagination
        [total]="items.totalCount"
        [offset]="items.offset">
      </pagination>
    </div>
  `
})
```

### Multiple Loadables

For components that need multiple async data sources, use the directive multiple times or combine streams beforehand.

## Components

### LoadingComponent

Default spinner shown during loading state. Uses Material `<mat-spinner>`.

### ErrorStateComponent

Default error UI with:

- Error icon
- Customizable title and message
- "Try Again" button (reloads page)

## Best Practices

1. **One loadable per major data dependency** - Don't nest multiple loadables if you can combine the streams
2. **Provide meaningful error messages** - Use the `errorMessage` input for user-friendly feedback
3. **Keep state services thin** - Let the directive handle loading/error UI
4. **Use toLoadable() liberally** - Convert any observable to get automatic loading/error handling
