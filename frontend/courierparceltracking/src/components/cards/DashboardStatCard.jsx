import { ArrowDownRight, ArrowUpRight, Minus } from 'lucide-react'
import '../../style/components/components.css'

function DashboardStatCard({
  accent = 'blue',
  description,
  disabled = false,
  icon: Icon,
  label,
  onClick,
  title = label,
  trend,
  trendDirection = 'neutral',
  value,
}) {
  const isInteractive = typeof onClick === 'function'
  const Card = isInteractive ? 'button' : 'article'
  const TrendIcon = trendDirection === 'positive'
    ? ArrowUpRight
    : trendDirection === 'negative'
      ? ArrowDownRight
      : Minus

  return (
    <Card
      className={`stat-card stat-card--${accent}${isInteractive ? ' stat-card--interactive' : ''}`}
      {...(isInteractive ? { disabled, onClick, type: 'button' } : {})}
    >
      {Icon && <span className="stat-card__icon"><Icon aria-hidden="true" size={21} /></span>}
      <p className="stat-card__label">{title}</p>
      <p className="stat-card__value">{value}</p>
      {description && (
        <p className="stat-card__description">
          <ArrowUpRight aria-hidden="true" size={15} />
          {description}
        </p>
      )}
      {trend && (
        <p className={`stat-card__trend stat-card__trend--${trendDirection}`}>
          <TrendIcon aria-hidden="true" size={15} />
          {trend}
        </p>
      )}
    </Card>
  )
}

export default DashboardStatCard
