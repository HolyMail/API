import Imap from 'imap';
import { Stream } from 'stream';
import { simpleParser, type ParsedMail } from 'mailparser';


const connectionPool = new Map<string, Imap>();
const mailCache = new Map<string, Map<number, ParsedMail>>();

type google_xoauth_login = {
    type: "google-oauth2",
    user: string,
    oauth2: string,
};

type apple_mail_password_login = {
    type: "apple-login",
    user: string,
    password: string,
};

type loginOptions = google_xoauth_login | apple_mail_password_login;

class ImapHandler {
    private login_options: loginOptions;
    private client?: Imap;

    constructor(login_options: loginOptions) {        
        this.login_options = login_options;

        this.Setup();
    }

    private Setup() {
        const { user } = this.login_options;

        const imapConfig: any = {
            user,
            port: 993,
            tls: true,
            tlsOptions: {
                rejectUnauthorized: false,
            }
        }

        switch (this.login_options.type) {
            case "google-oauth2": {
                const { oauth2 } = this.login_options;

                const xoauth2 = btoa(`user=${user}\u0001Auth=Bearer ${oauth2}\u0001\u0001`);

                imapConfig.xoauth2 = xoauth2;
                imapConfig.host = 'imap.gmail.com';

                break;
            }

            case "apple-login": {
                const { password } = this.login_options;

                imapConfig.password = password;
                imapConfig.host = 'imap.mail.me.com';

                break;
            }
        }

        if(connectionPool.has(user)) 
        {
            this.client = connectionPool.get(user);
        }
        else 
        {
            if(connectionPool.size >= 10) {
                const { done, value } = connectionPool.entries().next();

                if(!done) {
                    const [ user, connection ] = value;

                    connection.end();

                    connectionPool.delete(user);

                    mailCache.delete(user);
                }
            }

            this.client = new Imap(imapConfig);
            connectionPool.set(user, this.client);

            console.log("new connection created, pool size: " + connectionPool.size);
        }

        if (!this.client) {
            throw new Error("unreachable code, client should be initialized by now.");
        }

        this.client.once('end', () => console.log("connection closed"));
    }

    async Get(emails_number: number = -1, offset:number = 0) {
        if (!this.client) {
            throw new Error("Client not initialized");
        }

        await this.Connect();

        const box = await this.openInbox();

        console.log(box.messages.total + ' messages found!');

        const emails = await this.fetchEmails(emails_number, offset);

        return emails;
    }

    private async fetchEmails(emails_number: number = -1, offset: number = 0) {
        if (!this.client) {
            throw new Error("Client not initialized");
        }

        const end = emails_number !== -1 ? `${offset + emails_number}` : "*";

        const query = `${1+offset}:${end}`;
        const fetchOptions: Imap.FetchOptions = {
            bodies: "",
        };

        const fetch = this.client.fetch(query, fetchOptions);

        return await this.gatherMessages(fetch);
    }

    private Connect() {
        if(!this.client) {
            throw new Error("Client not initialized");
        }

        return new Promise((resolve, reject) => {
            this.client!.once('error', (err: any) => reject(err));
            this.client!.once('ready', resolve);

            this.client!.connect();
        })
    }

    private openInbox(): Promise<Imap.Box> {
        if (!this.client) {
            throw new Error("Client not initialized");
        }

        return new Promise((resolve, reject) => {
            this.client!.openBox('INBOX', true, (error, box) => {
                if (error) {
                    reject(error);
                }

                resolve(box);
            });
        });
    }

    private gatherMessages(fetch: Imap.ImapFetch): Promise<ParsedMail[]> {
        const mails: ParsedMail[] = [];

        const { user } = this.login_options;

        if(!mailCache.has(user)) {
            mailCache.set(user, new Map());
        }

        const userEmailCache = mailCache.get(user);

        const promises: Promise<ParsedMail>[] = [];
        
        return new Promise((resolve, reject) => {
            fetch.on('message', (message: Imap.ImapMessage, seqno: number) => {
                    message.on('body', async (stream: Stream) => {
                        try {
                            console.log("mail: " + seqno);
                            console.log("already parsed: " + userEmailCache!.has(seqno));
    
                            if(userEmailCache!.has(seqno)) {
                                const mail = userEmailCache!.get(seqno)!;
    
                                mails.push(mail);
                                return;
                            }
                            
                            const promise = simpleParser(stream);

                            promises.push(promise);

                            const mail = await promise;
                        
                            console.log("mail parsed: " + seqno);
    
                            mails.push(mail);
                            
                            userEmailCache!.set(seqno, mail);
                        }
                        catch (error) {
                            reject(error);
                        }
                    });
                });

            fetch.once('error', (err) => reject(err));

            fetch.once('end', async () => {
                await Promise.all(promises);
                console.log(userEmailCache!.size);

                resolve(mails)
            });
        });
    }
}

export {
    ImapHandler, type loginOptions as LoginOptions,
}