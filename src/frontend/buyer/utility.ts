import { EventSearchResult, PriceLevel, PriceList } from "./contracts/backendApi"
import { EventCardProps, PriceLevelProps } from "./contracts/props"

const formatLocaleId = 'en-UK'

export const Utility = {
    formatIsoDateTimeAsLongDate(isoDateTime: string): string {
        const date = new Date(Date.parse(isoDateTime))
        const formatted = date.toLocaleDateString(formatLocaleId, { year: 'numeric', month: 'long', day: 'numeric' })
        return formatted
    },
    formatIsoDateTimeAsShortTime(isoDateTime: string): string {
        const date = new Date(Date.parse(isoDateTime))
        const formatted = date.toLocaleTimeString(formatLocaleId, { hour: '2-digit', minute: '2-digit' })
        return formatted
    },
    formatNumberWithThousandSeparators(n: number): string {
        const formatted = n.toLocaleString(formatLocaleId)
        return formatted
    },
    formatIntegerFloorToThousands(n: number): string {
        const formatted = (n - (n % 1000)).toLocaleString(formatLocaleId)
        return formatted
    },
    eventSearchResultToCardProps(result: EventSearchResult): EventCardProps {
        const { eventId } = result
        const dateText = Utility.formatIsoDateTimeAsLongDate(result.eventStartTime)
        const showNameText = `${result.genreName} ${result.showName}`
        const eventNameText = result.troupes && result.troupes.length === 2
            ? `${result.troupes[0].name} - ${result.troupes[1].name}`
            : ''
        const priceText = `starting from $${parseInt(result.minPrice.toString())}`
        const seatsLeftBadgeText = `${Utility.formatNumberWithThousandSeparators(result.numberOfSeatsLeft)} left`
        const venueText = result.venueName
    
        return {
            eventId,
            dateText,
            showNameText,
            eventNameText,
            priceText,
            seatsLeftBadgeText,
            venueText
        }
    },
    joinPriceLevelsWithPriceList(priceLevels: PriceLevel[], priceList: PriceList): PriceLevelProps[] {
        return priceLevels.map(level => {
            const price = priceList.priceByLevelId[level.id]
            return {
                ...level,
                price
            }
        })
    }
}
