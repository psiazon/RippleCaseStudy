import React, { FormEvent, useMemo, useState } from 'react';
import { createRoot } from 'react-dom/client';
import { ApiConfig, ApiResult, sendRequest } from './api';
import './styles.css';

type TabName = 'events' | 'tickets';

type PricingTierForm = {
  name: string;
  price: string;
};

const defaultEventBaseUrl = 'https://localhost:5001';
const defaultTicketBaseUrl = 'https://localhost:6001';

function todayIsoDate(): string {
  const date = new Date();
  date.setDate(date.getDate() + 7);
  return date.toISOString().slice(0, 10);
}

function emptyEventForm() {
  return {
    id: '',
    name: 'UPSA Championship',
    description: 'Sample event created from React client.',
    venue: 'Wintrust Sports Complex',
    eventDate: todayIsoDate(),
    eventTime: '10:00',
    totalTicketCapacity: '500',
    pricingTiers: [
      { name: 'General Admission', price: '25' },
      { name: 'VIP', price: '50' }
    ] as PricingTierForm[]
  };
}

function App() {
  const [activeTab, setActiveTab] = useState<TabName>('events');
  const [config, setConfig] = useState<ApiConfig>({
    eventBaseUrl: defaultEventBaseUrl,
    ticketBaseUrl: defaultTicketBaseUrl,
    token: ''
  });
  const [result, setResult] = useState<ApiResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const run = async (action: () => Promise<ApiResult>) => {
    setLoading(true);
    setError(null);
    try {
      setResult(await action());
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Unexpected error');
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="app-shell">
      <header className="hero">
        <div>
          <p className="eyebrow">Ripple Ticketing Case Study</p>
          <h1>Event & Ticket Micro Service React Client</h1>
          <p>
            Use this UI to call the Event Management and Ticket Management HTTP endpoints and view formatted responses.
          </p>
        </div>
      </header>

      <section className="settings card">
        <h2>API Connection</h2>
        <div className="grid three">
          <label>
            Event API Base URL
            <input value={config.eventBaseUrl} onChange={(e) => setConfig({ ...config, eventBaseUrl: e.target.value })} />
          </label>
          <label>
            Ticket API Base URL
            <input value={config.ticketBaseUrl} onChange={(e) => setConfig({ ...config, ticketBaseUrl: e.target.value })} />
          </label>
          <label>
            Bearer JWT Token
            <input
              type="password"
              placeholder="Paste token without or with Bearer prefix"
              value={config.token}
              onChange={(e) => setConfig({ ...config, token: e.target.value })}
            />
          </label>
        </div>
      </section>

      <nav className="tabs" aria-label="Management tabs">
        <button className={activeTab === 'events' ? 'active' : ''} onClick={() => setActiveTab('events')}>Event Management</button>
        <button className={activeTab === 'tickets' ? 'active' : ''} onClick={() => setActiveTab('tickets')}>Ticket Management</button>
      </nav>

      <div className="workspace">
        {activeTab === 'events' ? <EventManagement config={config} run={run} /> : <TicketManagement config={config} run={run} />}
        <ResponsePanel result={result} loading={loading} error={error} />
      </div>
    </main>
  );
}

function EventManagement({ config, run }: { config: ApiConfig; run: (action: () => Promise<ApiResult>) => void }) {
  const [form, setForm] = useState(emptyEventForm());
  const requestBody = useMemo(() => ({
    name: form.name,
    description: form.description,
    venue: form.venue,
    eventDate: new Date(`${form.eventDate}T00:00:00`).toISOString(),
    eventTime: form.eventTime,
    totalTicketCapacity: Number(form.totalTicketCapacity),
    pricingTiers: form.pricingTiers.map((tier) => ({
      name: tier.name,
      price: Number(tier.price)
    }))
  }), [form]);

  const updateTier = (index: number, key: keyof PricingTierForm, value: string) => {
    setForm({
      ...form,
      pricingTiers: form.pricingTiers.map((tier, i) => (i === index ? { ...tier, [key]: value } : tier))
    });
  };

  const submit = (event: FormEvent, action: () => Promise<ApiResult>) => {
    event.preventDefault();
    run(action);
  };

  return (
    <section className="card operations">
      <h2>Event Management</h2>
      <p className="muted">Routes: GET/POST <code>/api/events</code>, GET/PUT/DELETE <code>/api/events/{'{id}'}</code></p>

      <form onSubmit={(e) => submit(e, () => sendRequest(config.eventBaseUrl, '/api/events', config.token))}>
        <h3>1. Get All Events</h3>
        <button type="submit">GET /api/events</button>
      </form>

      <form onSubmit={(e) => submit(e, () => sendRequest(config.eventBaseUrl, `/api/events/${form.id}`, config.token))}>
        <h3>2. Get Event By Id</h3>
        <label>Event Id <input value={form.id} onChange={(e) => setForm({ ...form, id: e.target.value })} /></label>
        <button type="submit">GET /api/events/{'{id}'}</button>
      </form>

      <form onSubmit={(e) => submit(e, () => sendRequest(config.eventBaseUrl, '/api/events', config.token, 'POST', requestBody))}>
        <h3>3. Create Event</h3>
        <EventFormFields form={form} setForm={setForm} updateTier={updateTier} />
        <button type="submit">POST /api/events</button>
      </form>

      <form onSubmit={(e) => submit(e, () => sendRequest(config.eventBaseUrl, `/api/events/${form.id}`, config.token, 'PUT', requestBody))}>
        <h3>4. Update Event</h3>
        <label>Event Id <input value={form.id} onChange={(e) => setForm({ ...form, id: e.target.value })} /></label>
        <EventFormFields form={form} setForm={setForm} updateTier={updateTier} compact />
        <button type="submit">PUT /api/events/{'{id}'}</button>
      </form>

      <form onSubmit={(e) => submit(e, () => sendRequest(config.eventBaseUrl, `/api/events/${form.id}`, config.token, 'DELETE'))}>
        <h3>5. Delete Event</h3>
        <label>Event Id <input value={form.id} onChange={(e) => setForm({ ...form, id: e.target.value })} /></label>
        <button type="submit" className="danger">DELETE /api/events/{'{id}'}</button>
      </form>
    </section>
  );
}

function EventFormFields({ form, setForm, updateTier, compact = false }: {
  form: ReturnType<typeof emptyEventForm>;
  setForm: React.Dispatch<React.SetStateAction<ReturnType<typeof emptyEventForm>>>;
  updateTier: (index: number, key: keyof PricingTierForm, value: string) => void;
  compact?: boolean;
}) {
  return (
    <>
      <div className="grid two">
        <label>Name <input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} /></label>
        <label>Venue <input value={form.venue} onChange={(e) => setForm({ ...form, venue: e.target.value })} /></label>
        <label>Event Date <input type="date" value={form.eventDate} onChange={(e) => setForm({ ...form, eventDate: e.target.value })} /></label>
        <label>Event Time <input type="time" value={form.eventTime} onChange={(e) => setForm({ ...form, eventTime: e.target.value })} /></label>
        <label>Total Capacity <input type="number" value={form.totalTicketCapacity} onChange={(e) => setForm({ ...form, totalTicketCapacity: e.target.value })} /></label>
      </div>
      {!compact && <label>Description <textarea value={form.description} onChange={(e) => setForm({ ...form, description: e.target.value })} /></label>}
      <div className="tier-header">
        <strong>Pricing Tiers</strong>
        <button type="button" className="secondary" onClick={() => setForm({ ...form, pricingTiers: [...form.pricingTiers, { name: '', price: '0' }] })}>Add Tier</button>
      </div>
      {form.pricingTiers.map((tier, index) => (
        <div className="grid two tier-row" key={index}>
          <label>Tier Name <input value={tier.name} onChange={(e) => updateTier(index, 'name', e.target.value)} /></label>
          <label>Price <input type="number" step="0.01" value={tier.price} onChange={(e) => updateTier(index, 'price', e.target.value)} /></label>
        </div>
      ))}
    </>
  );
}

function TicketManagement({ config, run }: { config: ApiConfig; run: (action: () => Promise<ApiResult>) => void }) {
  const [ticket, setTicket] = useState({
    eventId: '',
    pricingTierId: '',
    purchaserEmail: 'buyer@example.com',
    quantity: '1',
    totalCapacity: '500'
  });

  const purchaseBody = {
    eventId: ticket.eventId,
    pricingTierId: ticket.pricingTierId,
    purchaserEmail: ticket.purchaserEmail,
    quantity: Number(ticket.quantity)
  };

  const inventoryBody = {
    eventId: ticket.eventId,
    totalCapacity: Number(ticket.totalCapacity)
  };

  const submit = (event: FormEvent, action: () => Promise<ApiResult>) => {
    event.preventDefault();
    run(action);
  };

  return (
    <section className="card operations">
      <h2>Ticket Management</h2>
      <p className="muted">Routes: POST <code>/api/tickets/purchase</code>, POST <code>/api/tickets/inventories</code>, GET availability and reports.</p>

      <form onSubmit={(e) => submit(e, () => sendRequest(config.ticketBaseUrl, '/api/tickets/purchase', config.token, 'POST', purchaseBody))}>
        <h3>1. Purchase Ticket</h3>
        <div className="grid two">
          <label>Event Id <input value={ticket.eventId} onChange={(e) => setTicket({ ...ticket, eventId: e.target.value })} /></label>
          <label>Pricing Tier Id <input value={ticket.pricingTierId} onChange={(e) => setTicket({ ...ticket, pricingTierId: e.target.value })} /></label>
          <label>Purchaser Email <input type="email" value={ticket.purchaserEmail} onChange={(e) => setTicket({ ...ticket, purchaserEmail: e.target.value })} /></label>
          <label>Quantity <input type="number" value={ticket.quantity} onChange={(e) => setTicket({ ...ticket, quantity: e.target.value })} /></label>
        </div>
        <button type="submit">POST /api/tickets/purchase</button>
      </form>

      <form onSubmit={(e) => submit(e, () => sendRequest(config.ticketBaseUrl, '/api/tickets/inventories', config.token, 'POST', inventoryBody))}>
        <h3>2. Create Inventory</h3>
        <div className="grid two">
          <label>Event Id <input value={ticket.eventId} onChange={(e) => setTicket({ ...ticket, eventId: e.target.value })} /></label>
          <label>Total Capacity <input type="number" value={ticket.totalCapacity} onChange={(e) => setTicket({ ...ticket, totalCapacity: e.target.value })} /></label>
        </div>
        <button type="submit">POST /api/tickets/inventories</button>
      </form>

      <form onSubmit={(e) => submit(e, () => sendRequest(config.ticketBaseUrl, `/api/tickets/availability/${ticket.eventId}`, config.token))}>
        <h3>3. Check Ticket Availability</h3>
        <label>Event Id <input value={ticket.eventId} onChange={(e) => setTicket({ ...ticket, eventId: e.target.value })} /></label>
        <button type="submit">GET /api/tickets/availability/{'{eventId}'}</button>
      </form>

      <form onSubmit={(e) => submit(e, () => sendRequest(config.ticketBaseUrl, `/api/tickets/reports/sales/${ticket.eventId}`, config.token))}>
        <h3>4. Sales Report By Event Id</h3>
        <label>Event Id <input value={ticket.eventId} onChange={(e) => setTicket({ ...ticket, eventId: e.target.value })} /></label>
        <button type="submit">GET /api/tickets/reports/sales/{'{eventId}'}</button>
      </form>
    </section>
  );
}

function ResponsePanel({ result, loading, error }: { result: ApiResult | null; loading: boolean; error: string | null }) {
  return (
    <aside className="card response-panel">
      <h2>Response</h2>
      {loading && <div className="status loading">Calling API...</div>}
      {error && <div className="status error">{error}</div>}
      {!loading && !error && result && (
        <>
          <div className={result.ok ? 'status success' : 'status error'}>
            HTTP {result.status} {result.statusText || (result.ok ? 'Success' : 'Error')}
          </div>
          <p className="url">{result.url}</p>
          <FormattedData data={result.data} />
        </>
      )}
      {!loading && !error && !result && <p className="muted">Run an operation to see the HTTP response here.</p>}
    </aside>
  );
}

function FormattedData({ data }: { data: unknown }) {
  if (Array.isArray(data)) {
    return (
      <div className="data-list">
        {data.map((item, index) => <DataCard key={index} title={`Item ${index + 1}`} value={item} />)}
      </div>
    );
  }

  if (data && typeof data === 'object') {
    return <DataCard title="Response Body" value={data} />;
  }

  return <pre className="json-block">{data === null ? 'No response body' : String(data)}</pre>;
}

function DataCard({ title, value }: { title: string; value: unknown }) {
  if (!value || typeof value !== 'object' || Array.isArray(value)) {
    return <pre className="json-block">{JSON.stringify(value, null, 2)}</pre>;
  }

  return (
    <div className="data-card">
      <h3>{title}</h3>
      <dl>
        {Object.entries(value).map(([key, val]) => (
          <React.Fragment key={key}>
            <dt>{splitCamel(key)}</dt>
            <dd>{formatValue(val)}</dd>
          </React.Fragment>
        ))}
      </dl>
      <details>
        <summary>Raw JSON</summary>
        <pre className="json-block">{JSON.stringify(value, null, 2)}</pre>
      </details>
    </div>
  );
}

function formatValue(value: unknown): React.ReactNode {
  if (Array.isArray(value)) {
    return (
      <div className="chips">
        {value.map((item, index) => (
          <span className="chip" key={index}>{typeof item === 'object' ? JSON.stringify(item) : String(item)}</span>
        ))}
      </div>
    );
  }
  if (value && typeof value === 'object') {
    return <pre className="inline-json">{JSON.stringify(value, null, 2)}</pre>;
  }
  return String(value ?? '');
}

function splitCamel(value: string): string {
  return value.replace(/([A-Z])/g, ' $1').replace(/^./, (char) => char.toUpperCase());
}

createRoot(document.getElementById('root')!).render(<App />);
