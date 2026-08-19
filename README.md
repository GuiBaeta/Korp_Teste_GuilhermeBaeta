\# Sistema de Emissão de Notas Fiscais



Projeto desenvolvido como teste técnico para a Korp.



\## Tecnologias previstas



\### Backend



\- C#

\- ASP.NET Core Web API

\- Entity Framework Core

\- PostgreSQL



\### Frontend



\- Angular

\- Angular Material



\### Infraestrutura



\- Docker

\- Docker Compose



\## Arquitetura



A aplicação será composta inicialmente por dois microsserviços:



\- \*\*Inventory API\*\* — responsável por produtos e controle de estoque.

\- \*\*Billing API\*\* — responsável por notas fiscais e seus itens.



Cada microsserviço possuirá seu próprio banco de dados PostgreSQL.



```text

Inventory API -> Inventory DB



Billing API -> Billing DB

