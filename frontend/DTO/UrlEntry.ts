import { Credential } from "./Credential";

export interface UrlEntry {
    id: string,
    url: string;
    credentials: Credential[];
}