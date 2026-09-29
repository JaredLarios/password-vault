import { UrlEntry } from "./UrlEntry";

export interface Website {
  id: string;
  name: string;
  urls: UrlEntry[];
}