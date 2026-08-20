import { Component } from '@angular/core';

@Component({
  selector: 'app-invoices-page',
  template: `
    <section class="page-intro">
      <p class="eyebrow">Faturamento</p>
      <h1>Notas Fiscais</h1>
      <p>Acompanhe as notas emitidas e acesse o fluxo de criação e fechamento.</p>
    </section>
  `,
  styles: `
    .page-intro {
      max-width: 720px;
    }

    .eyebrow {
      margin: 0 0 6px;
      color: #4f46a5;
      font-size: 0.75rem;
      font-weight: 700;
      letter-spacing: 0.08em;
      text-transform: uppercase;
    }

    h1 {
      margin: 0;
      color: #1e293b;
      font-size: clamp(1.8rem, 4vw, 2.3rem);
      font-weight: 650;
      letter-spacing: -0.025em;
    }

    .page-intro > p:last-child {
      margin: 10px 0 0;
      color: #64748b;
      line-height: 1.6;
    }
  `
})
export class InvoicesPage {}
