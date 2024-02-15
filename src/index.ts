import { serve } from '@hono/node-server'
import { Hono } from 'hono';
import { ImapHandler, type LoginOptions } from './imap';


const app = new Hono();



app.post(`/emails/apple/list`, async (context) => {
  const { 
    user, 
    credentials,
    email_count,
    offset,
  } = await context.req.json();

  const imapConfig: LoginOptions = Object.freeze({
    type: "apple-login",
    user,
    password: credentials,
  });

  const imap = new ImapHandler(imapConfig);

  const emails = await imap.Get(email_count, offset);

  return context.json({
    data: emails,
  });
});

app.post(`/emails/apple/send`, async (context) => {
  const body = await context.req.json();

  return context.json({
    message: "emails"
  });
});



app.post(`/emails/google/list`, async (context) => {
  const { user, credentials } = await context.req.json();

  const imapConfig: LoginOptions = Object.freeze({
    type: "google-oauth2",
    user,
    oauth2: credentials,
  });

  const imap = new ImapHandler(imapConfig);

  const emails = await imap.Get(50);

  return context.json({
    data: emails,
  })
});

app.post(`/emails/google/send`, async (context) => {
  const body = await context.req.json();

  return context.json({
    message: "emails"
  });
});


const port = 3000
console.log(`Server is running on port ${port}`)

serve({
  fetch: app.fetch,
  port
})
