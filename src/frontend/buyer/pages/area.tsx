import React, { CSSProperties, useState } from 'react'
import { Alert, AlertIcon, Box, Button, Divider, Flex, FormControl, FormLabel, Grid, GridItem, HStack, Select, Slider, SliderFilledTrack, SliderMark, SliderThumb, SliderTrack, Tag, TagLabel, Text, VStack } from "@chakra-ui/react"

import { HeaderBar } from '../components/headerBar'
import { EventSearchAreaFullDetail, EventSearchAreaInfo, EventSearchFullDetail, EventSearchHallInfo, EventSearchResult, AreaSeatingMapSeat, AreaSeatingMap, AreaSeatingMapRow } from '../contracts/backendApi'
import { Utility } from '../utility'
import Link from 'next/link'
import { EventDetailsSidebar } from '../components/eventDetailsSidebar'
import { PriceLevelProps } from '../contracts/props'
import { CSSObject } from '@emotion/react'

const SeatCountSlider = (props: {
    value: number
    onChanged: (newValue: number) => void
}) => {
    const { value, onChanged } = props

    return (
        <Slider aria-label='slider-ex-6' mt='10' min={1} max={10} defaultValue={value} onChange={onChanged}>
            <SliderMark value={1} mt='3' ml='-1' fontSize='sm'>1</SliderMark>
            <SliderMark value={2} mt='3' ml='-1' fontSize='sm'>2</SliderMark>
            <SliderMark value={3} mt='3' ml='-1' fontSize='sm'>3</SliderMark>
            <SliderMark value={4} mt='3' ml='-1' fontSize='sm'>4</SliderMark>
            <SliderMark value={5} mt='3' ml='-1' fontSize='sm'>5</SliderMark>
            <SliderMark value={6} mt='3' ml='-1' fontSize='sm'>6</SliderMark>
            <SliderMark value={7} mt='3' ml='-1' fontSize='sm'>7</SliderMark>
            <SliderMark value={8} mt='3' ml='-1' fontSize='sm'>8</SliderMark>
            <SliderMark value={9} mt='3' ml='-1' fontSize='sm'>9</SliderMark>
            <SliderMark value={10} mt='3' ml='-1.5' fontSize='sm'>10</SliderMark>
            <SliderMark
                value={value}
                textAlign='center'
                bg='tomato'
                color='white'
                mt='-10'
                ml='-6'
                w='12'
            >
                {value}
            </SliderMark>
            <SliderTrack>
                <SliderFilledTrack bg='tomato' />
            </SliderTrack>
            <SliderThumb borderColor='tomato' borderWidth='3px' w='20px' h='20px' />
        </Slider>
    )
}

const SeatBox = (props: { 
    eventId: string
    areaId: string
    seat: AreaSeatingMapSeat
    priceLevels: PriceLevelProps[]
    isSelected: boolean
    onClicked: (seat: AreaSeatingMapSeat) => void
}) => {

    const { eventId, areaId, seat, priceLevels, isSelected, onClicked} = props

    const backgroundColor = seat.status === 1
        ? '#' + priceLevels.find(p => p.id === seat.priceLevelId)?.colorHexRgb
        : 'lightgray'
    const cursor = seat.status === 1
        ? 'pointer'
        : 'not-allowed'
    const border = isSelected
        ? '2px solid red'
        : 'none'
    const normalStyle: CSSProperties = { 
        backgroundColor, 
        cursor,
        border 
    }
    const hoverStyle: CSSObject = seat.status === 1
        ? { backgroundColor: 'red !important' }
        : { }

    return (
        <Box w='22px' h='20px' bg='lightgray' textAlign='center' verticalAlign='middle' p={0} m={0} color='white' borderRadius='sm' _hover={hoverStyle} style={normalStyle}>
            <Text fontSize='sm' fontWeight={600} p={0} m={0} onClick={() => onClicked(seat)}>{seat.name}</Text>
        </Box>
    )
}

const SeatingMap = (props: { 
    eventId: string
    seatingMap: AreaSeatingMap 
    priceLevels: PriceLevelProps[]
    selectedSeatIds: string[]
    onSeatClicked: (seat: AreaSeatingMapSeat, row: AreaSeatingMapRow) => void
}) => {
    const { eventId, seatingMap, priceLevels, selectedSeatIds, onSeatClicked } = props
    
    return (
        <VStack>
            {seatingMap.rows.map(row =>
                <HStack key={row.id} mt='2px !important'>
                    <Box w='20px' textAlign='end'>{row.name}</Box>
                    <Flex wrap='nowrap' justify='space-between' gap='2px'>
                        {row.seats.map(seat => <SeatBox 
                            key={seat.id} 
                            eventId={eventId} 
                            areaId={seatingMap.hallAreaId} 
                            seat={seat} 
                            priceLevels={priceLevels} 
                            onClicked={s => onSeatClicked(s, row)} 
                            isSelected={selectedSeatIds.indexOf(seat.id) >= 0} />)}
                    </Flex>
                </HStack>
            )}
        </VStack>
    )
}

interface AreaPageProps {
    data: EventSearchAreaFullDetail
}


const SeatingPlanSidebar = (props: AreaPageProps) => {
    const [selectedSeatIds, setSelectedSeatIds] = useState([])
    const [selectedSeatCount, setSelectedSeatCount] = useState(2)

    const priceLevelProps = Utility.joinPriceLevelsWithPriceList(props.data.priceLevels, props.data.header.priceList)

    const selectSeats = async (seat: AreaSeatingMapSeat, row: AreaSeatingMapRow) => {
        const indexFrom = row.seats.indexOf(seat)
        if (indexFrom + selectedSeatCount > row.seats.length) {
            return false;
        }

        const seatIds = []
        
        for (let i = indexFrom ; i < indexFrom + selectedSeatCount ; i++) {
            if (row.seats[i].status !== 1) {
                return false;
            }
            seatIds.push(row.seats[i].id)
        }

        setSelectedSeatIds(seatIds)
    }

    return (
        <>
            <HStack alignItems='start' mb='5px'>
            <Tag size={'lg'} borderRadius='2xl' variant='solid' colorScheme='green' mt='5px' ml='0' mr='10px' verticalAlign='start'>
                <TagLabel fontSize={'2xl'} fontWeight={700} mb='3px'>Area {props.data.hallAreaName}</TagLabel>
            </Tag>
            <Box minW='400px'>
                <Text fontSize='2xl' fontWeight={400}>
                    Let's grab your seats! How many?
                </Text>
                <Box w='full' pl='15px' pr='15px' mb='25px'>
                    <SeatCountSlider value={2} onChanged={setSelectedSeatCount} />
                </Box>
                <Box display='none'>
                    <Text fontSize='2xl' fontWeight={400}>
                        Now pick your first seat in the seat plan.
                    </Text>
                    <Text fontSize='1xl' mt='-5px' mb='10px' fontWeight={400}>
                        We will automatically pick the rest from left to right.
                    </Text>
                </Box>
            </Box>
        </HStack>
        <Alert status='warning' variant='left-accent' fontSize='18' fontWeight={400} lineHeight='1.0' mb='10px'>
            <AlertIcon />
            Oops, someone took those. We've refreshed the plan. Please try again.
        </Alert>
        <Alert display='none' status='success' variant='left-accent' fontSize='18' fontWeight={600} lineHeight='1.0' mb='10px'>
            <AlertIcon />
            The seats are yours! You now have
            <Tag size={'md'} borderRadius='full' variant='solid' colorScheme='orange' ml='7px' mr='7px' mt='3px' verticalAlign='middle'>
                <TagLabel fontSize={'lg'}>07:00</TagLabel>
            </Tag>                                       
            to 
            <Link href={`/event?id=${props.data.header.event.eventId}`}>
                <Button size='xs' colorScheme='green' fontWeight={400} fontSize='18' h='35px' fontWeight='500' ml='10px'>
                    <u>Complete Your Order</u>
                </Button>
            </Link>                                
        </Alert>                                    
        <SeatingMap 
            eventId={props.data.header.event.eventId} 
            seatingMap={props.data.seatingMap} 
            priceLevels={priceLevelProps} 
            selectedSeatIds={selectedSeatIds}
            onSeatClicked={selectSeats}/>
    </>)
}

const AreaPage = (props: AreaPageProps) => {
    return (
        <>
            <HeaderBar />
            <Grid 
                minH="100vh"
                templateRows="auto"
                templateColumns="auto 1200px auto"
            >
                <GridItem  h="full"></GridItem>
                <GridItem  h="full" pt='140px'>
                    <Flex wrap='nowrap' justify='space-between' gap='40px' align='stretch'>
                        <Box minW='200px' minH='400px'>
                            <EventDetailsSidebar fullDetail={props.data.header} />
                        </Box>
                        <Box minW='200px' minH='400px' fontSize='1xl'  color='blue.700'>
                            <SeatingPlanSidebar {...props} />
                        </Box>
                    </Flex>
                </GridItem>
                <GridItem  h="full"></GridItem>
            </Grid>  
        </>
    )
}

export default AreaPage

export async function getServerSideProps({ query }): Promise<{ props: AreaPageProps }> {
    const eventId = query.eventId
    const areaId = query.areaId
    if (typeof eventId !== 'string' || eventId.length < 10 || eventId.length > 40) {
        throw new Error('Invalid query string')
    }
    if (typeof areaId !== 'string' || areaId.length < 10 || areaId.length > 40) {
        throw new Error('Invalid query string')
    }

    try {
        const res = await fetch(`http://localhost:3001/search/event/${eventId}/area/${areaId}`)
        const envelope = await res.json()
        const data = envelope.data as EventSearchAreaFullDetail
        return { 
            props: { 
                data 
            }
        }
    } catch(err) {
        console.log(err)
        return { 
            props: {
                data: {} as any
            }
        }
    }
}
