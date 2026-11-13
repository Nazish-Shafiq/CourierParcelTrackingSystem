function DashboardSection({ action, children, description, title }) {
  return (
    <section className="dashboard-section">
      <header className="dashboard-section__header">
        <div>
          <h2 className="dashboard-section__title">{title}</h2>
          {description && <p className="dashboard-section__description">{description}</p>}
        </div>
        {action && <div className="dashboard-section__action">{action}</div>}
      </header>
      {children}
    </section>
  )
}

export default DashboardSection
