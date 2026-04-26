# Immagine Node 22 per sviluppo Angular con hot reload
FROM node:22-alpine

WORKDIR /app

# Installa le dipendenze nell'immagine (separate dal volume mount dell'host)
COPY package*.json ./
RUN npm ci

EXPOSE 4200

CMD ["npx", "ng", "serve", "--host", "0.0.0.0", "--proxy-config", "proxy.conf.docker.json"]
