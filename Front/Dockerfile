FROM node:slim
EXPOSE 4200

RUN mkdir /ng
WORKDIR /ng

RUN npm install -g @angular/cli

COPY OI.Web.Static/package.json OI.Web.Static/package-lock.json ./
#COPY package.json package-lock.json ./
RUN npm ci

COPY OI.Web.Static/. .
#COPY . .
CMD ["ng", "serve", "--host", "0.0.0.0"]
