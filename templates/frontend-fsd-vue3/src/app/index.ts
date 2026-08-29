import { createApp } from 'vue'
import { configureHttpAuthentication } from '@/shared/api'
import { useSessionStore } from '@/shared/auth'
import App from './App.vue'
import { pinia } from './pinia'
import { router } from './router'
import './styles/main.css'

const session = useSessionStore(pinia)

configureHttpAuthentication({
  getAuthorizationHeader: () => session.getAuthorizationHeader(),
  onUnauthorized: async () => {
    session.signOut()
    if (router.currentRoute.value.name !== 'sign-in') {
      await router.replace({
        name: 'sign-in',
        query: { redirect: router.currentRoute.value.fullPath },
      })
    }
  },
})

createApp(App).use(pinia).use(router).mount('#app')
