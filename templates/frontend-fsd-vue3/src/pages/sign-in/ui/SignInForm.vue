<script setup lang="ts">
import { computed, shallowRef } from 'vue'
import { storeToRefs } from 'pinia'
import { KeyRound, LogIn } from '@lucide/vue'
import { useRoute, useRouter } from 'vue-router'
import { useSessionStore, type AuthenticationMethod } from '@/shared/auth'

const route = useRoute()
const router = useRouter()
const session = useSessionStore()
const { authenticationError, isSigningIn } = storeToRefs(session)

const method = shallowRef<AuthenticationMethod>('basic')
const username = shallowRef('')
const password = shallowRef('')
const clientId = shallowRef('')
const usernameError = shallowRef('')
const passwordError = shallowRef('')

const submitLabel = computed(() =>
  method.value === 'basic' ? 'Sign in with Basic' : 'Request OAuth token',
)

async function submit(): Promise<void> {
  clearFeedback()
  if (!username.value.trim()) {
    usernameError.value = 'Enter a username.'
  }
  if (!password.value) {
    passwordError.value = 'Enter a password.'
  }
  if (usernameError.value || passwordError.value) {
    return
  }

  const credentials = {
    username: username.value.trim(),
    password: password.value,
  }
  const signedIn =
    method.value === 'basic'
      ? await session.signInBasic(credentials)
      : await session.signInOAuth({
          ...credentials,
          clientId: clientId.value.trim(),
        })

  if (signedIn) {
    await router.replace(resolveRedirect())
  }
}

function selectMethod(nextMethod: AuthenticationMethod): void {
  method.value = nextMethod
  clearFeedback()
}

function clearFeedback(): void {
  usernameError.value = ''
  passwordError.value = ''
  session.clearError()
}

function resolveRedirect(): string {
  const redirect = route.query.redirect
  return typeof redirect === 'string' &&
    redirect.startsWith('/') &&
    !redirect.startsWith('//')
    ? redirect
    : '/orders'
}
</script>

<template>
  <section class="sign-in-panel" aria-labelledby="sign-in-title">
    <div class="sign-in-panel__heading">
      <span class="sign-in-panel__icon" aria-hidden="true">
        <KeyRound :size="20" :stroke-width="1.8" />
      </span>
      <div>
        <h1 id="sign-in-title">Sign in</h1>
        <p>Secure access to Order Desk.</p>
      </div>
    </div>

    <div class="auth-methods" aria-label="Authentication method">
      <button
        type="button"
        :class="{ 'auth-methods__button--active': method === 'basic' }"
        :aria-pressed="method === 'basic'"
        @click="selectMethod('basic')"
      >
        Basic
      </button>
      <button
        type="button"
        :class="{ 'auth-methods__button--active': method === 'oauth' }"
        :aria-pressed="method === 'oauth'"
        @click="selectMethod('oauth')"
      >
        OAuth 2.0
      </button>
    </div>

    <form class="sign-in-form" @submit.prevent="submit">
      <label class="field">
        <span class="field__label">Username</span>
        <input
          v-model="username"
          name="username"
          type="text"
          autocomplete="username"
          :aria-invalid="Boolean(usernameError)"
          :disabled="isSigningIn"
          @input="clearFeedback"
        />
        <span v-if="usernameError" class="field__error">{{ usernameError }}</span>
      </label>

      <label class="field">
        <span class="field__label">Password</span>
        <input
          v-model="password"
          name="password"
          type="password"
          autocomplete="current-password"
          :aria-invalid="Boolean(passwordError)"
          :disabled="isSigningIn"
          @input="clearFeedback"
        />
        <span v-if="passwordError" class="field__error">{{ passwordError }}</span>
      </label>

      <label v-if="method === 'oauth'" class="field">
        <span class="field__label">Client ID <small>Optional</small></span>
        <input
          v-model="clientId"
          name="clientId"
          type="text"
          autocomplete="off"
          placeholder="public-client"
          :disabled="isSigningIn"
          @input="clearFeedback"
        />
      </label>

      <div v-if="authenticationError" class="sign-in-form__error" role="alert">
        <strong>Sign-in failed</strong>
        <span>{{ authenticationError.message }}</span>
        <span v-if="authenticationError.problem?.traceId" class="trace-id">
          Trace {{ authenticationError.problem.traceId }}
        </span>
      </div>

      <button class="primary-button" type="submit" :disabled="isSigningIn">
        <LogIn :size="17" :stroke-width="2" aria-hidden="true" />
        <span>{{ isSigningIn ? 'Signing in...' : submitLabel }}</span>
      </button>
    </form>
  </section>
</template>

<style scoped>
.sign-in-panel {
  width: min(100%, 420px);
  padding: 28px;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-surface);
}

.sign-in-panel__heading {
  display: flex;
  align-items: center;
  gap: 13px;
  margin-bottom: 24px;
}

.sign-in-panel__icon {
  display: inline-grid;
  width: 42px;
  height: 42px;
  flex: 0 0 42px;
  place-items: center;
  border: 1px solid #c8d4d8;
  border-radius: 6px;
  background: #eef4f4;
  color: #17656a;
}

.sign-in-panel__heading h1 {
  margin: 0;
  color: var(--color-text-strong);
  font-size: 21px;
}

.sign-in-panel__heading p {
  margin: 4px 0 0;
  color: var(--color-text-muted);
  font-size: 12px;
}

.auth-methods {
  display: grid;
  grid-template-columns: 1fr 1fr;
  padding: 3px;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: #f1f4f5;
}

.auth-methods__button--active,
.auth-methods button {
  min-height: 36px;
  border: 0;
  border-radius: 4px;
  background: transparent;
  color: var(--color-text-muted);
  cursor: pointer;
  font: inherit;
  font-size: 12px;
  font-weight: 700;
}

.auth-methods__button--active {
  background: #fff !important;
  color: #17656a !important;
  box-shadow: 0 1px 3px rgb(24 36 41 / 12%);
}

.sign-in-form {
  display: grid;
  gap: 18px;
  margin-top: 22px;
}

.field {
  display: grid;
  gap: 7px;
}

.field__label {
  color: var(--color-text);
  font-size: 12px;
  font-weight: 700;
}

.field__label small {
  margin-left: 4px;
  color: var(--color-text-muted);
  font-size: 10px;
  font-weight: 500;
}

.field input {
  width: 100%;
  min-height: 42px;
  padding: 0 11px;
  border: 1px solid #cbd4d7;
  border-radius: 5px;
  background: #fff;
  color: var(--color-text-strong);
  font: inherit;
  font-size: 13px;
  outline: none;
}

.field input:focus {
  border-color: #268087;
  box-shadow: 0 0 0 3px rgb(38 128 135 / 14%);
}

.field input[aria-invalid='true'] {
  border-color: #b64343;
}

.field__error {
  color: #a33232;
  font-size: 12px;
}

.sign-in-form__error {
  display: grid;
  gap: 4px;
  padding: 11px 12px;
  border-left: 3px solid #b64343;
  background: #fff3f2;
  color: #7e2929;
  font-size: 12px;
}

.trace-id {
  font-family: var(--font-mono);
  font-size: 11px;
}

.primary-button {
  display: inline-flex;
  min-height: 42px;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 0 15px;
  border: 1px solid #17656a;
  border-radius: 5px;
  background: #17656a;
  color: #fff;
  cursor: pointer;
  font: inherit;
  font-size: 13px;
  font-weight: 700;
}

.primary-button:disabled {
  cursor: wait;
  opacity: 0.65;
}
</style>
