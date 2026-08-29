<script setup lang="ts">
import SignInForm from './SignInForm.vue'
</script>

<template>
  <div class="sign-in-page">
    <SignInForm />
  </div>
</template>

<style scoped>
.sign-in-page {
  display: grid;
  min-height: calc(100vh - 61px);
  place-items: center;
  padding: 32px 18px 64px;
}
</style>
